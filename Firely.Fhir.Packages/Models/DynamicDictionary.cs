using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Firely.Fhir.Packages
{
    // The class derived from DynamicObject.
    public class DynamicDictionary : DynamicObject
    {
        // The inner dictionary.
        readonly Dictionary<string, object> dictionary = new();

        // If you try to get a value of a property
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return dictionary.TryGetValue(binder.Name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            dictionary[binder.Name] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames() => dictionary.Keys;

   //     IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dictionary).GetEnumerator();
    }

}
