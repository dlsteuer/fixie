﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Fixie.Conventions;
using Fixie.Results;
using Should;

namespace Fixie.Tests.Results
{
    public class ExceptionInfoTests
    {
        readonly AssertionLibraryFilter assertionLibrary = new AssertionLibraryFilter();

        public void ShouldSummarizeTheGivenException()
        {
            var exception = GetNestedException();

            var exceptionInfo = new ExceptionInfo(exception, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Type.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Message.ShouldEqual("Null reference!");
            exceptionInfo.StackTrace.ShouldEqual(exception.StackTrace);

            exceptionInfo.InnerException.DisplayName.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.InnerException.Type.ShouldEqual("System.DivideByZeroException");
            exceptionInfo.InnerException.Message.ShouldEqual("Divide by zero!");
            exceptionInfo.InnerException.StackTrace.ShouldEqual(exception.InnerException.StackTrace);

            exceptionInfo.InnerException.InnerException.ShouldBeNull();
        }

        public void ShouldSummarizeCollectionsOfExceptionsComprisedOfPrimaryAndSecondaryExceptions()
        {
            var primaryException = GetNestedException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryNestedException();

            var exceptionInfo = new ExceptionInfo(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Type.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Message.ShouldEqual("Null reference!");
            exceptionInfo.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
                    "Null reference!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by zero!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: System.ArgumentException =====",
                    "Argument!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryNestedException() in " + PathToThisFile() + ":line #",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not implemented!",
                    "   at Fixie.Tests.Results.ExceptionInfoTests.GetSecondaryNestedException() in " + PathToThisFile() + ":line #");

            exceptionInfo.InnerException.ShouldBeNull();
        }

        public void ShouldFilterAssertionLibraryImplementationDetails()
        {
            var primaryException = GetNestedException();
            var secondaryExceptionA = new NotImplementedException();
            var secondaryExceptionB = GetSecondaryNestedException();

            assertionLibrary
                .Namespace("Fixie.Tests.Results")
                .Namespace("System");

            var exceptionInfo = new ExceptionInfo(new[] { primaryException, secondaryExceptionA, secondaryExceptionB }, assertionLibrary);

            exceptionInfo.DisplayName.ShouldEqual("");
            exceptionInfo.Type.ShouldEqual("System.NullReferenceException");
            exceptionInfo.Message.ShouldEqual("Null reference!");
            exceptionInfo.StackTrace
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(x => Regex.Replace(x, @":line \d+", ":line #")) //Avoid brittle assertion introduced by stack trace line numbers.
                .ShouldEqual(
                    "Null reference!",
                    "",
                    "",
                    "------- Inner Exception: System.DivideByZeroException -------",
                    "Divide by zero!",
                    "",
                    "",
                    "===== Secondary Exception: System.NotImplementedException =====",
                    "The method or operation is not implemented.",
                    "",
                    "",
                    "===== Secondary Exception: System.ArgumentException =====",
                    "Argument!",
                    "",
                    "",
                    "------- Inner Exception: System.ApplicationException -------",
                    "Application!",
                    "",
                    "",
                    "------- Inner Exception: System.NotImplementedException -------",
                    "Not implemented!",
                    "");

            exceptionInfo.InnerException.ShouldBeNull();
        }

        static Exception GetNestedException()
        {
            try
            {
                try
                {
                    throw new DivideByZeroException("Divide by zero!");
                }
                catch (Exception exception)
                {
                    throw new NullReferenceException("Null reference!", exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        static Exception GetSecondaryNestedException()
        {
            try
            {
                try
                {
                    try
                    {
                        throw new NotImplementedException("Not implemented!");
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException("Application!", exception);
                    }
                }
                catch (Exception exception)
                {
                    throw new ArgumentException("Argument!", exception);
                }
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }
    }
}
