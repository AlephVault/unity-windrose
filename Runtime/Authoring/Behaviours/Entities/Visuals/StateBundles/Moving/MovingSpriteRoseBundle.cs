using AlephVault.Unity.WindRose.Types;

namespace AlephVault.Unity.WindRose
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
                            public class MovingSpriteRoseBundle : SpriteRoseBundle
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
