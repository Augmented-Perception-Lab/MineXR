using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorId
{
    public string stringValue { get; set; }
}

public class ComponentImage
{
    public string stringValue { get; set; }
}

public class Document
{
    public string name { get; set; }
    public Fields fields { get; set; }
    public System.DateTime createTime { get; set; }
    public System.DateTime updateTime { get; set; }
}

public class Fields
{
    public ComponentImage componentImage { get; set; }
    public FuncName funcName { get; set; }
    public AnchorId anchorId { get; set; }
}

public class FuncName
{
    public string stringValue { get; set; }
}

public class Root
{
    public List<Document> documents { get; set; }
}

public class Anchor
{
    public string anchorId { get; set; }
    public string anchorKey { get; set; }
    public string componentImagePath { get; set; }
    public GameObject gameObject { get; set; }

}