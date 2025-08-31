using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum MemoryType { Working, Episodic, Semantic, Social }

[Serializable]
public class MemoryRecord
{
    public string id;
    public MemoryType type;
    public string content;
    public string[] tags;
    public System.DateTime timestamp;
    public float importance;
    public int occurrences;
    public float[] embedding;
}