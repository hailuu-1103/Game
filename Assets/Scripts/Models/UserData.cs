using System.Collections.Generic;
using UnityEngine.UI;

namespace Models
{
    [System.Serializable]
    public class UserData
    {
        /* For Load Game */
        
        
        /* For Distribution */
        public int PlayTimes { get; set; }
        public float WinPercentage { get; set; }
        public int CurrentSteak { get; set; }
        public int MaxSteak { get; set; }
        
    }
}
