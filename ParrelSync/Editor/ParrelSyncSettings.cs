using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ParrelSync.Editor
{
    public class ParrelSyncSettings : ScriptableObject
    {
        public List<string> cloneProjectPath = new List<string>();
    }
}