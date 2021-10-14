using ChilliSource.Cloud.Core;
using Stripe;
using System;

namespace ChilliCoreTemplate.Service
{
    public partial class StripeService
    {
        public ServiceResult<Coupon> Coupon_Get(string id)
        {
            try
            {
                var service = new CouponService(_client);
                var response = service.Get(id);
                return ServiceResult<Coupon>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<Coupon>.AsError(ex.Message);
            }
        }

        public ServiceResult<PromotionCode> PromotionCode_Get(string id)
        {
            try
            {
                var service = new PromotionCodeService(_client);
                var response = service.Get(id);
                return ServiceResult<PromotionCode>.AsSuccess(response);
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<PromotionCode>.AsError(ex.Message);
            }
        }

        public ServiceResult<PromotionCode> PromotionCode_GetByCode(string code)
        {
            try
            {
                var service = new PromotionCodeService(_client);
                var response = service.List(new PromotionCodeListOptions { Code = code, Active = true });
                if (response.Data.Count == 1) return ServiceResult<PromotionCode>.AsSuccess(response.Data[0]);
                return ServiceResult<PromotionCode>.AsError("Promotion code not found");
            }
            catch (Exception ex)
            {
                if (!(ex is StripeException))
                {
                    ex.LogException();
                }
                return ServiceResult<PromotionCode>.AsError(ex.Message);
            }
        }
    }
}
