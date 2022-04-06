// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System;
using System.Collections.Generic;

namespace EasyPeasy.Core
{
    public class EasyPeasyException : Exception
    {
        public EasyPeasyException() : base()
        {

        }
        public EasyPeasyException(string message) : base(message)
        {

        }
    }

    public class ControllerException : Exception
    {
        public ControllerException() : base()
        {

        }
        public ControllerException(string message) : base(message)
        {

        }
    }

    public class ControllerNotFoundException : Exception
    {
        public ControllerNotFoundException() : base()
        {

        }
        public ControllerNotFoundException(string message) : base(message)
        {

        }
    }

    public class ControllerBadRequestException : Exception
    {
        public ControllerBadRequestException() : base()
        {

        }
        public ControllerBadRequestException(string message) : base(message)
        {

        }
    }

    public class ControllerUnauthorizedException : Exception
    {
        public ControllerUnauthorizedException() : base()
        {

        }
        public ControllerUnauthorizedException(string message) : base(message)
        {

        }
    }

    public class EasyPeasyDirectoryTraversalException : Exception
    {
        public EasyPeasyDirectoryTraversalException() : base()
        {

        }
        public EasyPeasyDirectoryTraversalException(string message) : base(message)
        {

        }
    }

    public class EasyPeasyLauncherNeedsListenerException : EasyPeasyException
    {
        public EasyPeasyLauncherNeedsListenerException() : base()
        {

        }
        public EasyPeasyLauncherNeedsListenerException(string message) : base(message)
        {

        }
    }

    public class EasyPeasyCompileGrawlStagerFailedException : EasyPeasyException
    {
        public EasyPeasyCompileGrawlStagerFailedException() : base()
        {

        }
        public EasyPeasyCompileGrawlStagerFailedException(string message) : base(message)
        {

        }
    }
}
