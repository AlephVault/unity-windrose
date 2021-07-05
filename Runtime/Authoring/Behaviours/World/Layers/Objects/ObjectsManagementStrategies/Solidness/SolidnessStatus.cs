namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Objects
                    {
                        namespace ObjectsManagementStrategies
                        {
                            namespace Solidness
                            {
                                /// <summary>
                                ///   Solidness status involves the ability of objects to be completely solid (i.e. not traverse anything,
                                ///     but also not allow other solid objects to traverse them), ghost objects (they can traverse and be
                                ///     traversed), and "hole" objects: they occupy negative space, so when overlapping with solid objects,
                                ///     those solid objects become traversable in the overlapped area. A 4th style is the MASK one, that
                                ///     allows us to specify an irregular pattern for the object's solidness.
                                /// </summary>
                                public enum SolidnessStatus { Hole, Ghost, Solid, Mask };

                                public static class SolidnessStatusMethods
                                {
                                    /// <summary>
                                    ///   Solid objects cannot traverse anything. Others will.
                                    /// </summary>
                                    /// <param name="status"></param>
                                    /// <returns></returns>
                                    public static bool Traverses(this SolidnessStatus status)
                                    {
                                        switch (status)
                                        {
                                            case SolidnessStatus.Solid:
                                                return false;
                                            default:
                                                return true;
                                        }
                                    }

                                    /// <summary>
                                    ///   Solid, and solid-for-others objects occupy space. Others don't.
                                    /// </summary>
                                    /// <param name="status"></param>
                                    /// <returns></returns>
                                    public static bool Occupies(this SolidnessStatus status)
                                    {
                                        switch (status)
                                        {
                                            case SolidnessStatus.Solid:
                                                return true;
                                            default:
                                                return false;
                                        }
                                    }

                                    /// <summary>
                                    ///   Hole objects produce negative occupancy. Others don't.
                                    /// </summary>
                                    /// <param name="status"></param>
                                    /// <returns></returns>
                                    public static bool Carves(this SolidnessStatus status)
                                    {
                                        switch (status)
                                        {
                                            case SolidnessStatus.Hole:
                                                return true;
                                            default:
                                                return false;
                                        }
                                    }

                                    /// <summary>
                                    ///   Masks are irregular. They cannot move.
                                    /// </summary>
                                    /// <param name="status"></param>
                                    /// <returns></returns>
                                    public static bool Irregular(this SolidnessStatus status)
                                    {
                                        switch (status)
                                        {
                                            case SolidnessStatus.Mask:
                                                return true;
                                            default:
                                                return false;
                                        }
                                    }

                                    /// <summary>
                                    ///   Tells whether the occupancy/carving quality of the compared values
                                    ///     is different or not.
                                    /// </summary>
                                    public static bool OccupancyChanges(this SolidnessStatus oldStatus, SolidnessStatus newStatus)
                                    {
                                        return (oldStatus.Occupies()) != newStatus.Occupies() || (oldStatus.Carves() != newStatus.Carves());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
