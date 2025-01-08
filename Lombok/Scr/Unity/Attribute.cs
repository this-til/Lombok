using System;
using System.Collections.Generic;

namespace Til.Lombok.Unity {


    public class NetworkSerializationClassAttribute : ClassAttribute {

        public NetworkSerializationClassAttribute() {
        }

        public NetworkSerializationClassAttribute(Dictionary<string, string> data) : base(data) {
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NetworkSerializationFieldAttribute : FieldAttribute {

        public NetworkSerializationFieldAttribute() {
        }

        public NetworkSerializationFieldAttribute(Dictionary<string, string> data) : base(data) {
        }

    }


}
