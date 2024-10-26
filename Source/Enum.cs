using System;

namespace Til.Lombok {

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

    public enum AccessLevel  {

        
        Public = 0,
        
        Private,

        Protected,

        ProtectedInternal,

        Internal,

    }

    public enum MethodType {

        def = 0,

        Abstract,
        Partial,
        
        Override,
        Virtual,
    }

    public enum PartialPos {

        Interior,
        UpLevel,
        Compilation,
        Namespace

    }



}
