using System;

namespace Til.Lombok {
    

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
