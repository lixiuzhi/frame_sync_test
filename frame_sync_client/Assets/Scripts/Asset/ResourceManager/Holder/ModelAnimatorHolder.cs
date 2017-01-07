using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModelAnimatorHolder : MonoBehaviour {

    [System.Serializable]
    public class AnimatorControllerPair
    {
        //set RuntimeAnimatorController
        public Animator animator;
        public string controllerABName;
    }

    public List<AnimatorControllerPair> animatorCtrs;

}
