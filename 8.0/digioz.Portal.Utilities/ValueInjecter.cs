using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace digioz.Portal.Utilities
{
    public static class ValueInjecter
    {
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest, List<string> excludes = null) {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                            .Where(x => x.CanWrite)
                            .ToList();

            foreach (var sourceProp in sourceProps) {
                if (destProps.Any(x => x.Name == sourceProp.Name)) {
                    try {
                        var p = destProps.First(x => x.Name == sourceProp.Name);

                        // check if the property can be set or no.
                        if (p.CanWrite && !excludes.Contains(p.Name)) { 
                            p.SetValue(dest, sourceProp.GetValue(source, null), null);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
