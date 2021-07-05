using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameMeanMachine.Unity.WindRose
{
    namespace Authoring
    {
        namespace ScriptableObjects
        {
            namespace Tiles
            {
                /// <summary>
                ///   This is an attribute to be used on Tile Strategies. This attribute ensures a tile strategy
                ///     requires another one, because their functionalities are dependent somehow. This requirement
                ///     is both in runtime and as documentation.
                /// </summary>
                [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
                public class RequireTileStrategy : AlephVault.Unity.Layout.Utils.Assets.Depends
                {
                    public RequireTileStrategy(Type dependency) : base(dependency) { }

                    protected override Type BaseDependency()
                    {
                        return typeof(Strategies.TileStrategy);
                    }
                }
            }
        }
    }
}
