using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace _.Scripts.Extensions
{
    public static class ICollectionExtension
    {
        public static T Random<T>(this ICollection<T> enumerable, Random random)
        {
            var valuesCount = enumerable.Count();
            var randomIndex = random.NextInt(0, valuesCount);

            return enumerable.ElementAt(randomIndex);
        }
    }
}