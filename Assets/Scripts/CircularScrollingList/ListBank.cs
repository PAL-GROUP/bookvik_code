/* Store the contents for ListBoxes to display.
 */
using UnityEngine;

public class ListBank : MonoBehaviour
{
    public static ListBank Instance;

    private int[] contents;

    void Start()
    {
		var _contents =	GetComponent<CircilarScrollController>().Elements.ToArray();
		contents = new int [_contents.Length];
		for(int i = 0; i < _contents.Length;i++)
        {
			contents[i] = int.Parse(_contents[i].content.text);
        }
    }

    public string getListContent(int index)
    {
        return contents[index].ToString("00");
    }

    public int getListLength()
    {
        return contents.Length;
    }
}
