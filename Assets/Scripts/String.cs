using UnityEngine;

public class String : MonoBehaviour
{
    private void Start()
    {
        // prints hello
        string s = "hello";
        Debug.Log(s);

        // prints hello world
        s = string.Format("{0} {1}", s, "world");
        Debug.Log(s);

        // string interpolation 
        s = $"{s} world";
        Debug.Log(s);

        // prints helloworld
        s = string.Concat("hello", "world");
        Debug.Log(s);

        // prints HELLOWORLD
        s = s.ToUpper();
        Debug.Log(s);

        // prints helloworld
        s = s.ToLower();
        Debug.Log(s);

        // prints 'e'
        Debug.Log(s[1]);

        // prints 42
        int i = 42;
        s = i.ToString();
        Debug.Log(s);

        // prints -43
        s = "-43";
        i = int.Parse(s);
        Debug.Log(i);

        // is null or empty
        Debug.Log(string.IsNullOrEmpty(""));

        // is null or whitespace
        Debug.Log(string.IsNullOrWhiteSpace(" "));

        // customize log with tags
        Debug.Log("<color=red>This text is red</color>");
        Debug.Log("<size=20>This text is big</size>");
    }
}