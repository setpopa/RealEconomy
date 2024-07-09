using System.Collections.Generic;
using SDG.Unturned;
using System.Linq;

namespace RealEconomy.HelpMethods
{
    internal class HelpMethod
    {
        private static RealEconomy pluginInstance => RealEconomy.Instance;

        public static List<InventorySearch> FindSubsetWithTargetSum(List<InventorySearch> objects, long targetSum) //kindly equired from CHATGPT :DDDD
        {
            
            // Sort objects by value to prioritize smaller values
            objects = objects.OrderBy(o => pluginInstance._idAmount[o.jar.item.id]).ToList();

            List<InventorySearch> result = new List<InventorySearch>();
            if (FindSubsetRecursive(objects, targetSum, 0, result))
            {
                return result;
            }
            return null;
        }

        private static bool FindSubsetRecursive(List<InventorySearch> objects, long targetSum, int startIndex, List<InventorySearch> currentSubset)
        {
            if (targetSum == 0)
            {
                return true;
            }
            if (targetSum < 0)
            {
                return false;
            }

            for (int i = startIndex; i < objects.Count; i++)
            {
                currentSubset.Add(objects[i]);
                if (FindSubsetRecursive(objects, targetSum - pluginInstance._idAmount[objects[i].jar.item.id], i + 1, currentSubset))
                {
                    return true;
                }
                currentSubset.RemoveAt(currentSubset.Count - 1); // Backtrack
            }
            return false;
        }
    }
}

