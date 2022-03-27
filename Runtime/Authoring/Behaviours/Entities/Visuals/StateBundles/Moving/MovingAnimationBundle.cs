using GameMeanMachine.Unity.WindRose.Types;

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
                            public class MovingAnimationBundle : AnimationBundle
                            {
                                // The moving state.
                                private static readonly State MOVING_STATE = State.Get("moving");

                                protected override State GetState()
                                {
                                    return MOVING_STATE;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
