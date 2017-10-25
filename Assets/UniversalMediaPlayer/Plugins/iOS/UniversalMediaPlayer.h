#import <Foundation/Foundation.h>
#import <OpenGLES/ES2/gl.h>
#import <OpenGLES/ES2/glext.h>
#import <UIKit/UIKit.h>
#import <UniversalMediaPlayer/UniversalMediaPlayer.h>
#import "UnityAppController.h"

#include <vector>
#include "UnityMetalSupport.h"

enum PlayerState
{
    Empty,
    Opening,
    Buffering,
    Prepared,
    Playing,
    Paused,
    Stopped,
    EndReached,
    EncounteredError,
    TimeChanged,
    PositionChanged,
    SnapshotTaken
};

@interface NSMutableArray (QueueStack)
-(PlayerState)queuePop;
-(void)queuePush:(PlayerState)obj;
@end

@interface UniversalMediaPlayer : NSObject

@property int renderingAPI;
@property id renderingDevice;
@property UniversalMediaPlayer *instance;
@property (atomic, retain) id<MediaPlayback> player;
@property NSMutableArray *playerEvents;
@property intptr_t texturePointer;
@property void *textureCache;
@property void *texture;
@property bool isBuffering;
@property NSString *videoPath;
@property int cachedVolume;
@property float cachedRate;

- (void)initMediaPlayer;
- (CVPixelBufferRef)videoBuffer;
- (bool)frameAvailable;

- (void)setDataSource:(NSString *)aUrl;
- (void)play;
- (void)pause;
- (void)stop;
- (void)shutdown;
- (int)getDuration;
- (int)getVolume;
- (void)setVolume:(int)value;
- (int)getTime;
- (void)setTime:(int)value;
- (float)getPosition;
- (void)setPosition:(float)value;
- (bool)isPlaying;
- (bool)isReady;
- (float)getPlaybackRate;
- (void)setPlaybackRate:(float)value;
- (int)getVideoWidth;
- (int)getVideoHeight;

@end
