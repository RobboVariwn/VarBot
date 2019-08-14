using System;
using System.Collections.Generic;
using System.Linq;
using Varwin;
using UnityEditor;
using UnityEngine;

[Serializable]
public class BlocklyConfig
{
    public string Guid { get; set; }  
    public string RootGuid { get; set; }  
    public List<string> Tags { get; set; }
    public bool Embedded { get; set; }
    public bool MobileReady { get; set; }
    public Config Config { get; set; }
    public JsonAuthor Author { get; set; }
    public JsonLicense License { get; set; }
    public string BuiltAt { get; set; }
    public string SdkVersion { get; set; }
}

[Serializable]
public class JsonAuthor
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Url { get; set; }
}

[Serializable]
public class JsonLicense
{
    public string Code { get; set; }
    public string Version { get; set; }
}

[Serializable]
public class Config : ILocalizable
{
    public string type { get; set; }
    public I18n i18n { get; set; }
    public List<Block> blocks { get; set; }
}

[Serializable]
public class I18n
{
    public string ru { get; set; }
    public string en { get; set; }
}

[Serializable]
public class Item : ILocalizable
{
    public string method { get; set; }
    public I18n i18n { get; set; }
    public string property { get; set; }
    public string name { get; set; }
}

[Serializable]
public class Param : ILocalizable
{
    public string name { get; set; }
    public string valueType { get; set; }
    public I18n i18n { get; set; }
}

[Serializable]
public class Block
{
    public string name { get; set; }
    public string type { get; set; }
    public List<Item> items { get; set; }
    public string valueType { get; set; }
    public List<Param> @params { get; set; }
    public List<Arg> args { get; set; }
    public List<Value> values { get; set; }
    
    public void AddItem(Item item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
        }
    }
}

[Serializable]
public class Value : ILocalizable
{
    public string name { get; set; }
    public I18n i18n { get; set; }
}

[Serializable]
public class Arg : ILocalizable
{
    public string values { get; set; }
    public string valueType { get; set; }
    public I18n i18n { get; set; }
}

public interface ILocalizable
{
    I18n i18n { get; set; }
}
