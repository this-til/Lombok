using System;

namespace Til.Lombok {

    /// <summary>
    /// The kind of members which Lombok.NET supports.
    /// </summary>
    public enum MemberType {

        /// <summary>
        /// Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// A C# field.
        /// </summary>
        Field,

        /// <summary>
        /// A C# property.
        /// </summary>
        Property

    }

    /// <summary>
    /// The kinds of accesses Lombok.NET supports.
    /// </summary>
    [Flags]
    public enum AccessTypes {

        /// <summary>
        /// Associated with the private keyword.
        /// </summary>
        Private,

        /// <summary>
        /// Associated with the protected keyword.
        /// </summary>
        Protected,

        /// <summary>
        /// Associated with the internal keyword.
        /// </summary>
        Internal,

        /// <summary>
        /// Associated with the public keyword.
        /// </summary>
        Public

    }

    public enum AccessLevel {

        Private,

        Protected,

        ProtectedInternal,

        Internal,

        Public

    }

    public enum MethodType {

        def,

        Abstract,
        Partial,
        
        Override,
        Virtual,
    }

    public class A {

    }

}
