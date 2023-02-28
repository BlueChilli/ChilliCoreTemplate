using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Stripe;
using ChilliSource.Cloud.Core;
using Microsoft.EntityFrameworkCore.Internal;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {

        public ServiceResult<Customer> Customer_AddOrUpdate(StripeCustomerEditModel model, string accountId = null)
        {
            if (!model.Metadata.ContainsKey(SYSTEM)) model.Metadata.Add(SYSTEM, _config.BaseUrl);

            Customer customer = null;
            if (!String.IsNullOrEmpty(model.Id))
            {
                var getRequest = Customer_Get(model.Id, accountId: accountId);
                if (!getRequest.Success) return ServiceResult<Customer>.CopyFrom(getRequest);
                customer = getRequest.Result;
            }
            else if (!String.IsNullOrEmpty(model.Email))
            {
                var lookupRequest = Customer_Find(model.Email, accountId);
                if (!lookupRequest.Success) return ServiceResult<Customer>.CopyFrom(lookupRequest);
                customer = lookupRequest.Result;
            }

            if (customer == null)
            {
                return Customer_Create(new CustomerCreateOptions
                {
                    Email = model.Email,
                    Description = model.Description,
                    Metadata = model.Metadata,
                    Source = model.Token
                }, accountId);
            }

            model.Email = model.Email ?? customer.Email;
            if (String.IsNullOrEmpty(model.Token) && String.IsNullOrEmpty(model.Card) && model.Email == customer.Email && model.Description == customer.Description)
                return ServiceResult<Customer>.AsSuccess(customer);

            return Customer_Update(customer.Id, new CustomerUpdateOptions
            {
                Email = model.Email,
                Description = model.Description,
                Metadata = model.Metadata,
                Source = model.Token,
                DefaultSource = model.Card
            }, accountId);
        }

        public ServiceResult<Customer> Customer_Get(string id, CustomerGetOptions options = null, string accountId = null)
        {
            try
            {
                if (String.IsNullOrEmpty(id)) return ServiceResult<Customer>.AsError("Customer not set up for stripe");
                var service = new CustomerService(_client);
                options = options ?? new CustomerGetOptions { Expand = new List<string> { "default_source", "subscriptions" } };
                var customer = service.Get(id, options, requestOptions: CreateRequestOptions(accountId));
                return ServiceResult<Customer>.AsSuccess(customer);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Customer>.AsError(ex.Message);
            }
        }

        public ServiceResult<Customer> Customer_Find(string email, string accountId = null)
        {
            try
            {
                var service = new CustomerService(_client);
                var customers = service.List(new CustomerListOptions { Email = email }, CreateRequestOptions(accountId));
                if (customers.Count() > 1) return ServiceResult<Customer>.AsError($"Mutiple accounts ({customers.Count()}) found for {email}. Use customer id to specify");
                return ServiceResult<Customer>.AsSuccess(customers.Any() ? customers.First() : null);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Customer>.AsError(ex.Message);
            }
        }

        public ServiceResult<Customer> Customer_Create(CustomerCreateOptions options, string accountId = null)
        {
            try
            {
                var service = new CustomerService(_client);
                var customer = service.Create(options, CreateRequestOptions(accountId));
                return ServiceResult<Customer>.AsSuccess(customer);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Customer>.AsError(ex.Message);
            }
        }

        public ServiceResult<Customer> Customer_Update(string id, CustomerUpdateOptions options, string accountId = null)
        {
            try
            {
                var service = new CustomerService(_client);
                var customer = service.Update(id, options, CreateRequestOptions(accountId));
                return ServiceResult<Customer>.AsSuccess(customer);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Customer>.AsError(ex.Message);
            }
        }

        public ServiceResult<Customer> Customer_Delete(string id)
        {
            try
            {
                var service = new CustomerService(_client);
                var customer = service.Delete(id);
                return ServiceResult<Customer>.AsSuccess(customer);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Customer>.AsError(ex.Message);
            }
        }
    }
}
