using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AlephVault.Unity.WindRose
{
    namespace MenuActions
    {
        namespace Tiles
        {
            static class TileUtils
            {
                public static T[] GetSelectedAssets<T>() where T : Object
                {
                    return Selection.GetFiltered<T>(SelectionMode.Unfiltered);
                }
            }
        }
    }
}
