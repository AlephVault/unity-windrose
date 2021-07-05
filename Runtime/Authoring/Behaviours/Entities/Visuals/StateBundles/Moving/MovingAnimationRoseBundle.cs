using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities
            {
                namespace Visuals
                {
                    namespace StateBundles
                    {
                        namespace Moving
                        {
                            public class MovingAnimationRoseBundle : AnimationRoseBundle
                            {
                                protected override string GetStateKey()
                                {
                                    return "moving";
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
