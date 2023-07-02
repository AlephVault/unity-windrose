using System;
using System.Reflection;
using UnityEngine;

namespace AlephVault.Unity.WindRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities.Common
            {
                /// <summary>
                ///   This behaviour implements a broadcast for pause/resume methods, which will be
                ///     executed even if the object is inactive.
                /// </summary>
                public class Pausable : MonoBehaviour
                {
                    /// <summary>
                    ///   This interface provides methods to pause/resume the objects.
                    ///     Several 
                    /// </summary>
                    public interface IPausable
                    {
                        void Pause(bool fullFreeze);
                        void Resume();
                    }

                    /// <summary>
                    ///   Pauses the object. This will imply that its behaviours will all
                    ///     pause accordinly.
                    /// </summary>
                    /// <param name="fullFreeze">Whether also pause animations or not</param>
                    public void Pause(bool fullFreeze)
                    {
                        foreach (IPausable behaviour in GetComponents<IPausable>())
                        {
                            behaviour.Pause(fullFreeze);
                        }
                    }

                    /// <summary>
                    ///   Resumes (releases) the object.
                    /// </summary>
                    public void Resume()
                    {
                        foreach (IPausable behaviour in GetComponents<IPausable>())
                        {
                            behaviour.Resume();
                        }
                    }
                }
            }
        }
    }
}
