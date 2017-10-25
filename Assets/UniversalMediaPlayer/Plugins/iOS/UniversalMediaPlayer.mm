#include "UniversalMediaPlayer.h"

@implementation NSMutableArray (QueueStack)
-(PlayerState)queuePop {
    @synchronized(self)
    {
        if ([self count] == 0) {
            return Empty;
        }
        
        PlayerState queueObject = (PlayerState)[[self objectAtIndex:0] intValue];
        [self removeObjectAtIndex:0];
        
        return queueObject;
    }
}

-(void)queuePush:(PlayerState)anObject {
    @synchronized(self)
    {
        [self addObject:@(anObject)];
    }
}
@end

@implementation UniversalMediaPlayer : NSObject

- (id)init
{
    self = [super init];
    _playerEvents = [NSMutableArray array];
    _cachedVolume = -1;
    _cachedRate = -1;
    _instance = self;
    
    _renderingAPI = (int)UnitySelectedRenderingAPI();
    _renderingDevice = UnityGetMetalDevice();
    if (_renderingAPI != apiMetal)
        _renderingDevice = UnityGetMainScreenContextGLES();
    
    return _instance;
}

- (void)initMediaPlayer
{
	[FFMoviePlayerController setLogReport:NO];
    [FFMoviePlayerController setLogLevel:k_LOG_SILENT];
    [FFMoviePlayerController checkIfFFmpegVersionMatch:NO];
    
    FFOptions *options = [FFOptions optionsByDefault];
    [options setPlayerOptionIntValue:1 forKey:@"start-on-prepared"];
    [options setPlayerOptionIntValue:0 forKey:@"framedrop"];
    [options setPlayerOptionIntValue:1 forKey:@"videotoolbox"];
    
    int api = (int)UnitySelectedRenderingAPI();
    id device = UnityGetMetalDevice();
    if (api != apiMetal)
        device = UnityGetMainScreenContextGLES();
    
    _player = [[FFMoviePlayerController alloc] initWithOptions:options];
    
    _player.scalingMode = MMPMovieScalingModeAspectFit;
    
    [self installMovieNotificationObservers];

}

- (bool)frameAvailable
{
    return [_player isReady];
}

- (CVPixelBufferRef)videoBuffer
{
    return [_player videoBuffer];
}

- (void)setDataSource:(NSString *)aUrl
{
    _videoPath = aUrl;
}

- (void)play
{
    if (_player == nil)
        [self initMediaPlayer];
    
    
    if (![_player isPreparedToPlay])
    {
        [_player setDataSourceURL:[NSURL URLWithString:_videoPath]];
        [_player prepareToPlay];
    }
    else
        [_player play];
}

- (void)pause
{
    [_player pause];
}

- (void)stop
{
    if (_player != nil)
    {
        [_player shutdown];
        _cachedVolume = [_player playbackVolume];
        _cachedRate = [_player playbackRate];
        
        [self removeMovieNotificationObservers];
        _player = nil;
        
        if(_texture)
        {
            if (_renderingAPI == apiMetal)
                CFRelease(_texture);
            else
            {
                unsigned tex = (unsigned)[self getGLTextureFromCVTextureCache:_texture];
                glDeleteTextures(1, &tex);
            }
            _texture = 0;
        }
        
        if(_textureCache)
        {
            CFRelease(_textureCache);
            _textureCache = 0;
        }
    }
}

- (void)shutdown
{
    [self stop];
}

- (int)getDuration
{
    return [_player duration] * 1000;
}

- (int)getVolume
{
    if (![_player isReady])
        return _cachedVolume;
    else
        return [_player playbackVolume] * 100;
}

- (void)setVolume:(int)value
{
    if (![_player isReady])
        _cachedVolume = value;
    else
        [_player setPlaybackVolume:(float)value / 100.0];
}

- (int)getTime
{
    return [_player currentPlaybackTime] * 1000;
}

- (void)setTime:(int)value
{
    [_player setCurrentPlaybackTime:value];
}

- (float)getPosition
{
    float position = [_player currentPlaybackTime] / [_player duration];
    return position;
}

- (void)setPosition:(float)value
{
    float position = [_player duration] * value;
    [_player setCurrentPlaybackTime:position];
}

- (bool)isPlaying
{
    return [_player isPlaying];
}

- (bool)isReady
{
    return [_player isPreparedToPlay];
}

- (float)getPlaybackRate
{
    if (![_player isReady])
        return _cachedRate;
    else
        return [_player playbackRate];
}

- (void)setPlaybackRate:(float)value
{
    if (![_player isReady])
        _cachedRate = value;
    else
        [_player setPlaybackRate:value];
}

- (int)getVideoWidth
{
    return [_player videoWidth];
}

- (int)getVideoHeight
{
    return [_player videoHeight];
}

- (void)movieVideoDecoderOpen:(NSNotification*)notification
{
    [_playerEvents queuePush:Opening];
}

- (void)loadStateDidChange:(NSNotification*)notification
{
    switch (_player.loadState)
    {
        case MMPMovieLoadStateStalled:
            _isBuffering = true;
            break;
        
        case MMPMovieLoadStatePlayable | MMPMovieLoadStatePlaythroughOK:
            _isBuffering = false;
            break;
    }
}

- (void)moviePlayBackDidFinish:(NSNotification*)notification
{
    int reason = [[[notification userInfo] valueForKey:MMPMoviePlayerPlaybackDidFinishReasonUserInfoKey] intValue];
    
    switch (reason)
    {
        case MMPMovieFinishReasonPlaybackEnded:
            [_playerEvents queuePush:EndReached];
            break;
            
        case MMPMovieFinishReasonUserExited:
            break;
            
        case MMPMovieFinishReasonPlaybackError:
            [_playerEvents queuePush:EncounteredError];
            break;
    }
}

- (void)mediaIsPreparedToPlayDidChange:(NSNotification*)notification
{
    NSLog(@"mediaIsPreparedToPlayDidChange\n");
}

- (void)moviePlayBackStateDidChange:(NSNotification*)notification
{
    switch (_player.playbackState)
    {
        case MMPMoviePlaybackStateStopped:
            [_playerEvents queuePush:Stopped];
            break;
            
        case MMPMoviePlaybackStatePlaying:
            [_playerEvents queuePush:Playing];
            break;
            
        case MMPMoviePlaybackStatePaused:
            [_playerEvents queuePush:Paused];
            break;
            
        case MMPMoviePlaybackStateInterrupted:
            break;
            
        case MMPMoviePlaybackStateSeekingForward:
        case MMPMoviePlaybackStateSeekingBackward:
            break;
    
        default:
            break;
    }
}

#pragma mark Install Movie Notifications

/* Register observers for the various movie object notifications. */
-(void)installMovieNotificationObservers
{
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(loadStateDidChange:)
                                                 name:MMPMoviePlayerLoadStateDidChangeNotification
                                               object:_player];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(moviePlayBackDidFinish:)
                                                 name:MMPMoviePlayerPlaybackDidFinishNotification
                                               object:_player];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(mediaIsPreparedToPlayDidChange:)
                                                 name:MMPMediaPlaybackIsPreparedToPlayDidChangeNotification
                                               object:_player];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(moviePlayBackStateDidChange:)
                                                 name:MMPMoviePlayerPlaybackStateDidChangeNotification
                                               object:_player];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(movieVideoDecoderOpen:)
                                                 name:MMPMoviePlayerVideoDecoderOpenNotification
                                               object:_player];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(systemVolumeChanged:)
                                                 name:@"AVSystemController_SystemVolumeDidChangeNotification"
                                               object:nil];
}

#pragma mark Remove Movie Notification Handlers

/* Remove the movie notification observers from the movie object. */
-(void)removeMovieNotificationObservers
{
    [[NSNotificationCenter defaultCenter]removeObserver:self name:MMPMoviePlayerLoadStateDidChangeNotification object:_player];
    [[NSNotificationCenter defaultCenter]removeObserver:self name:MMPMoviePlayerPlaybackDidFinishNotification object:_player];
    [[NSNotificationCenter defaultCenter]removeObserver:self name:MMPMediaPlaybackIsPreparedToPlayDidChangeNotification object:_player];
    [[NSNotificationCenter defaultCenter]removeObserver:self name:MMPMoviePlayerPlaybackStateDidChangeNotification object:_player];
    [[NSNotificationCenter defaultCenter]removeObserver:self name:MMPMoviePlayerVideoDecoderOpenNotification object:_player];
    
    [[NSNotificationCenter defaultCenter]removeObserver:self name:@"AVSystemController_SystemVolumeDidChangeNotification" object:nil];
}

- (uintptr_t)getGLTextureFromCVTextureCache:(void*)texture
{
    if (_renderingAPI == apiMetal)
        return (uintptr_t)(__bridge void*)CVMetalTextureGetTexture((CVMetalTextureRef)texture);
    else
        return CVOpenGLESTextureGetName((CVOpenGLESTextureRef)texture);
}

- (void*)createCVTextureCache
{
    void* ret = 0;
    
    CVReturn err = 0;
    if(_renderingAPI == apiMetal)
        err = CVMetalTextureCacheCreate(kCFAllocatorDefault, 0, _renderingDevice, 0, (CVMetalTextureCacheRef*)&ret);
    else
        err = CVOpenGLESTextureCacheCreate(kCFAllocatorDefault, 0, _renderingDevice, 0, (CVOpenGLESTextureCacheRef*)&ret);
    
    if (err)
    {
        NSLog(@"Error at CVOpenGLESTextureCacheCreate: %d", err);
        ret = 0;
    }
    
    return ret;
}

- (void*)createTextureFromCVTextureCache:(void*)cache image:(void*)image width:(unsigned)w height:(unsigned)h
{
    void* texture = 0;
    
    CVReturn err = 0;
    if(_renderingAPI == apiMetal)
    {
        err = CVMetalTextureCacheCreateTextureFromImage(
                                                        kCFAllocatorDefault, (CVMetalTextureCacheRef)cache, (CVImageBufferRef)image, 0,
                                                        MTLPixelFormatBGRA8Unorm, w, h, 0, (CVMetalTextureRef*)&texture
                                                        );
    }
    else
    {
        err = CVOpenGLESTextureCacheCreateTextureFromImage(
                                                           kCFAllocatorDefault, (CVOpenGLESTextureCacheRef)cache, (CVImageBufferRef)image, 0,
                                                           GL_TEXTURE_2D, GL_RGBA, w, h, GL_BGRA_EXT, GL_UNSIGNED_BYTE,
                                                           0, (CVOpenGLESTextureRef*)&texture
                                                           );
    }
    
    if (err)
    {
        NSLog(@"Error at CVOpenGLESTextureCacheCreateTextureFromImage: %d", err);
        texture = 0;
    }
    
    return texture;
}
@end

static std::vector<UniversalMediaPlayer*> _players;

static NSString* CreateNSString(const char* string)
{
    if (string != NULL)
        return [NSString stringWithUTF8String:string];
    else
        return [NSString stringWithUTF8String:""];
}

extern "C"
{    
    int NativeInit()
    {
        _players.push_back([[UniversalMediaPlayer alloc] init]);
        return (int)_players.size();
    }
    
    intptr_t NativeGetTexturePointer(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        if (player.textureCache == nil)
            player.textureCache = [player createCVTextureCache];
        
        player.texture = [player createTextureFromCVTextureCache:player.textureCache image:[player videoBuffer] width:[player getVideoWidth] height:[player getVideoHeight]];
        
        [player.playerEvents queuePush:PositionChanged];
        [player.playerEvents queuePush:TimeChanged];
        
        return [player getGLTextureFromCVTextureCache:player.texture];
    }
	
	bool NativeFrameAvailable(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        if (player.isBuffering)
            [player.playerEvents queuePush:Buffering];
        
        if (player.frameAvailable)
        {
            if (player.cachedVolume >= 0)
            {
                [player setVolume:player.cachedVolume];
                player.cachedVolume = -1;
            }
            if (player.cachedRate >= 0)
            {
                [player setPlaybackRate:player.cachedRate];
                player.cachedRate = -1;
            }
        }
        
        return [player frameAvailable];
    }
    
    void NativeUpdateTexture(int index, intptr_t texture) {}
    
    void SetDataSource(int index, char *aUrl)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        NSString *pathUrl = CreateNSString(aUrl);
        [player setDataSource:pathUrl];
    }
    
    bool Play(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player play];
        return true;
    }
    
    void Pause(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player pause];
    }
    
    void Stop(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player stop];
    }
    
    void Release(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player shutdown];
    }
	
	bool IsPlaying(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player isPlaying];
    }
    
    bool IsReady(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player isReady];
    }
    
    int GetLength(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getDuration];
    }
	
	float GetBufferingPercentage(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        float buffProgress = player.player.bufferingProgress;
        
        if (buffProgress >= 100.0f)
            buffProgress = 100.0f;
        
        return buffProgress;
    }
	
	int GetTime(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getTime];
    }
    
    void SetTime(int index, int time)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player setTime:time];
    }
    
    float GetPosition(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getPosition];
    }
    
    void SetPosition(int index, float position)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player setPosition:position];
    }
	
	float GetRate(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getPlaybackRate];
    }
    
	void SetRate(int index, float rate)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        if (rate != player.getPlaybackRate)
            [player setPlaybackRate:rate];
    }
    
    int GetVolume(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getVolume];
    }
    
    void SetVolume(int index, int value)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        [player setVolume:value];
    }
	
	bool GetMute(UniversalMediaPlayer *mpObj)
    {
        return false;
    }
	
	void SetMute(int index, bool state)
    {
    }
    
    int VideoWidth(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getVideoWidth];
    }
    
    int VideoHeight(int index)
    {
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        return [player getVideoHeight];
    }
	
	int GetState(int index)
	{
        UniversalMediaPlayer *player = _players.at(index - 1);
        
        if (player.playerEvents.count > 0)
        {
            PlayerState state = player.playerEvents.queuePop;
            return state;
        }
        
        return Empty;
	}
}
