using ChilliCoreTemplate.Models.Api;
using ChilliCoreTemplate.Service;
using ChilliSource.Cloud.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Web.Api
{
    public static class ControllerExtensionsForApi
    {
        [DebuggerNonUserCodeAttribute]
        public static IApiServiceCallerSyntax<T> ApiServiceCall<T>(this ControllerBase controller, Func<ServiceResult<T>> action)
        {
            return (new ApiServiceCaller<T>(controller)).SetAction(action);
        }

        [DebuggerNonUserCodeAttribute]
        public static IApiServiceCallerSyntax<ApiPagedList<T>> ApiServiceCall<T>(this ControllerBase controller, Func<ApiPagedList<T>> action)
        {
            Func<ServiceResult<ApiPagedList<T>>> wrappedAction = () => ServiceResult<ApiPagedList<T>>.AsSuccess(action());

            return (new ApiServiceCaller<ApiPagedList<T>>(controller)).SetAction(wrappedAction);
        }

        [DebuggerNonUserCodeAttribute]
        public static IApiServiceCallerSyntax<List<T>> ApiServiceCall<T>(this ControllerBase controller, Func<List<T>> action)
        {
            Func<ServiceResult<List<T>>> wrappedAction = () => ServiceResult<List<T>>.AsSuccess(action());

            return (new ApiServiceCaller<List<T>>(controller)).SetAction(wrappedAction);
        }

        [DebuggerNonUserCodeAttribute]
        public static IApiServiceCallerAsyncSyntax<T> ApiServiceCall<T>(this ControllerBase controller, Func<Task<ServiceResult<T>>> action)
        {
            return (new ApiServiceCallerAsync<T>(controller)).SetAction(action);
        }

        [DebuggerNonUserCodeAttribute]
        public static IApiServiceCallerAsyncSyntax<ApiPagedList<T>> ApiServiceCall<T>(this ControllerBase controller, Func<Task<ApiPagedList<T>>> action)
        {
            Func<Task<ServiceResult<ApiPagedList<T>>>> wrappedAction = async () =>
            {
                var result = await action();
                return ServiceResult<ApiPagedList<T>>.AsSuccess(result);
            };

            return (new ApiServiceCallerAsync<ApiPagedList<T>>(controller)).SetAction(wrappedAction);
        }
    }

    public interface IApiServiceCallerSyntax<T>
    {
        IApiServiceCallerSyntax<T> OnSuccess(Func<ServiceResult<T>, IActionResult> onSuccess);
        IApiServiceCallerSyntax<T> OnFailure(Func<IActionResult> onFailure);
        IApiServiceCallerSyntax<T> OnFailure(Func<ServiceResult<T>, IActionResult> onFailure);
        IActionResult Call();
    }

    public interface IApiServiceCallerAsyncSyntax<T>
    {
        IApiServiceCallerAsyncSyntax<T> OnSuccess(Func<ServiceResult<T>, Task<IActionResult>> onSuccess);
        IApiServiceCallerAsyncSyntax<T> OnFailure(Func<Task<IActionResult>> onFailure);
        IApiServiceCallerAsyncSyntax<T> OnFailure(Func<ServiceResult<T>, Task<IActionResult>> onFailure);

        IApiServiceCallerAsyncSyntax<T> OnSuccess(Func<ServiceResult<T>, IActionResult> onSuccess);
        IApiServiceCallerAsyncSyntax<T> OnFailure(Func<IActionResult> onFailure);
        IApiServiceCallerAsyncSyntax<T> OnFailure(Func<ServiceResult<T>, IActionResult> onFailure);
        Task<IActionResult> Call();
    }

    [DebuggerNonUserCodeAttribute]
    internal class ApiServiceCaller<T> : IApiServiceCallerSyntax<T>
    {
        Func<ServiceResult<T>> _action;
        Func<ServiceResult<T>, IActionResult> _onSuccess;
        Func<ServiceResult<T>, IActionResult> _onFailure;
        ControllerBase _controller;

        public ApiServiceCaller(ControllerBase controller)
        {
            _controller = controller;
            //default action for success;
            if (typeof(T) == typeof(object))
            {
                this.OnSuccess((response) => (response.Result == null ? (IActionResult)_controller.Ok()
                                                                        : (IActionResult)_controller.Ok(response.Result)));
            }
            else
            {
                this.OnSuccess((response) => _controller.Ok(response.Result));
            }

            //default action for failure;
            this.OnFailure((response) => _controller.CreateApiErrorResponse(response, response.StatusCode));
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerSyntax<T> SetAction(Func<ServiceResult<T>> action)
        {
            _action = action; return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerSyntax<T> OnSuccess(Func<ServiceResult<T>, IActionResult> onSuccess)
        {
            _onSuccess = onSuccess; return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerSyntax<T> OnFailure(Func<IActionResult> onFailure)
        {
            _onFailure = (T) => onFailure(); return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerSyntax<T> OnFailure(Func<ServiceResult<T>, IActionResult> onFailure)
        {
            _onFailure = onFailure; return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IActionResult Call()
        {
            if (_action == null || _onSuccess == null || _onFailure == null)
            {
                throw new ApplicationException("You need to set up the service call.");
            }

            if (!_controller.ModelState.IsValid)
            {
                return _controller.CreateApiErrorResponse(_controller.ModelState, HttpStatusCode.BadRequest);
            }

            ServiceResult<T> response = _action();
            if (response.Success)
            {
                return _onSuccess(response);
            }

            return _onFailure(response);
        }
    }

    [DebuggerNonUserCodeAttribute]
    internal class ApiServiceCallerAsync<T> : IApiServiceCallerAsyncSyntax<T>
    {
        Func<Task<ServiceResult<T>>> _action;
        Func<ServiceResult<T>, Task<IActionResult>> _onSuccess;
        Func<ServiceResult<T>, Task<IActionResult>> _onFailure;
        ControllerBase _controller;

        public ApiServiceCallerAsync(ControllerBase controller)
        {
            _controller = controller;
            //default action for success;
            if (typeof(T) == typeof(object))
            {
                this.OnSuccess((response) => (response.Result == null ? (IActionResult)_controller.Ok()
                                                                        : (IActionResult)_controller.Ok(response.Result)));
            }
            else
            {
                this.OnSuccess((response) => _controller.Ok(response.Result));
            }

            //default action for failure;
            this.OnFailure((response) => _controller.CreateApiErrorResponse(response, response.StatusCode));
        }

        [DebuggerNonUserCodeAttribute]
        public ApiServiceCallerAsync<T> SetAction(Func<Task<ServiceResult<T>>> action)
        {
            _action = action; return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerAsyncSyntax<T> OnSuccess(Func<ServiceResult<T>, Task<IActionResult>> onSuccess)
        {
            _onSuccess = onSuccess; return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerAsyncSyntax<T> OnFailure(Func<Task<IActionResult>> onFailure)
        {
            _onFailure = (T) => onFailure(); return this;
        }

        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerAsyncSyntax<T> OnFailure(Func<ServiceResult<T>, Task<IActionResult>> onFailure)
        {
            _onFailure = onFailure; return this;
        }

        //Sync continuation
        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerAsyncSyntax<T> OnSuccess(Func<ServiceResult<T>, IActionResult> onSuccess)
        {
            _onSuccess = (T) => Task.FromResult(onSuccess(T)); return this;
        }

        //Sync continuation
        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerAsyncSyntax<T> OnFailure(Func<IActionResult> onFailure)
        {
            _onFailure = (T) => Task.FromResult(onFailure()); return this;
        }

        //Sync continuation
        [DebuggerNonUserCodeAttribute]
        public IApiServiceCallerAsyncSyntax<T> OnFailure(Func<ServiceResult<T>, IActionResult> onFailure)
        {
            _onFailure = (T) => Task.FromResult(onFailure(T)); return this;
        }

        [DebuggerNonUserCodeAttribute]
        public async Task<IActionResult> Call()
        {
            if (_action == null || _onSuccess == null || _onFailure == null)
            {
                throw new ApplicationException("You need to set up the service call.");
            }

            if (!_controller.ModelState.IsValid)
            {
                return _controller.CreateApiErrorResponse(_controller.ModelState, HttpStatusCode.BadRequest);
            }

            ServiceResult<T> response = await _action();
            if (response.Success)
            {
                return await _onSuccess(response);
            }

            return await _onFailure(response);
        }
    }

    internal static class ApiControllerExtensions
    {
        public static IActionResult CreateApiErrorResponse(this ControllerBase controller, ModelStateDictionary modelState, HttpStatusCode statusCode)
        {
            var obj = ErrorResult.Create(modelState);

            return new ObjectResult(obj) { StatusCode = (int)statusCode };
        }

        public static IActionResult CreateApiErrorResponse<T>(this ControllerBase controller, ServiceResult<T> serviceResult, HttpStatusCode statusCode)
        {
            var obj = ErrorResult.Create(serviceResult);

            return new ObjectResult(obj) { StatusCode = (int)statusCode };
        }

        public static IActionResult CreateApiErrorResponse(this ControllerBase controller, IEnumerable<string> errors, HttpStatusCode statusCode)
        {
            var obj = ErrorResult.Create(errors);
            
            return new ObjectResult(obj) { StatusCode = (int)statusCode };
        }
    } 
}
