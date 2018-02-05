using System;
using Nop.Core;
using Nop.Core.Infrastructure;

namespace Nop.Services.Catalog {
    /// <summary>
    /// Extensions
    /// </summary>
    public static class RoundingHelper {
        /// <summary>
        /// Round a product or order total
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <returns>Rounded value</returns>
        public static decimal RoundPrice(decimal value) {
            //we use this method because some currencies (e.g. Gungarian Forint or Swiss Franc) use non-standard rules for rounding
            //you can implement any rounding logic here

            var workContext = EngineContext.Current.Resolve<IWorkContext>();

            return value.Round();
        }

        /// <summary>
        /// Round
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="roundingType">The rounding type</param>
        /// <returns>Rounded value</returns>
        public static decimal Round(this decimal value) {
            //default round (Rounding001)
            var rez = Math.Round(value, 2);
            var fractionPart = (rez - Math.Truncate(rez)) * 10;

            //cash rounding not needed
            if (fractionPart == 0)
                return rez;

            return rez;
        }
    }
}
