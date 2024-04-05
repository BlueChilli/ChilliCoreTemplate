using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
   
    public static class Conditions
    {
        public static ConditionModel CustomerInformationAdvice(string companyName, string companyAddress, string phone)
        {
            var model = new ConditionModel
            {
                Conditions = [
                    new()
                    {
                        Content = $"<p><strong>Name and Contact Details of the Funeral Service Supplier</strong></p>{companyName} Postal:{companyAddress} Phone: {phone}"
                    },
                ]
            };
            return model;
        }

        public readonly static ConditionModel TermsOfAgreement = new()
        {
            Conditions = [
                new()
                {
                    Content = "<strong>This Pre-Paid Funeral Plan Contract (the ‘Contract’) evidences the legal agreement between the Funeral Director and the Contract Holder and Beneficiary</strong> and is subject to the following provisions:",
                    Conditions = [
                        new() { Content = "the Contract Holder agrees to pay to the Funeral Director the Total Payable listed on the Contract. The Contract Holder acknowledges his/her obligation to pay this amount in full within the time frame indicated in the Contract’s Terms or Agreement. The Contract Holder acknowledges his/her understanding that, following the expiry of the Cooling-Off Period described in the Terms of Agreement of this Contract, no monies may be withdrawn from the amount paid prior to the provision of the Funeral Service; and" },
                        new() { Content = "the Funeral Director agrees to provide the services as outlined in the Contract in accordance with the Terms of Agreement together with all applicable laws and regulations. The Funeral Director agrees to deposit all funds paid towards the Total Payable (the ‘Investment Amount’) under the Contract with Australian Funeral Fund Management Pty Ltd (the ‘Funeral Fund Manager’), less any fees and taxes withheld or returned to the Funeral Director by the Funeral Fund Manager as permissible under the Contract’s Terms of Agreement and applicable laws and regulations; and" },
                        new() { Content = "the Funeral Fund Manager does not guarantee payment to the Funeral Director of any amounts paid towards the Investment Amount and the Funeral Fund’s investment performance may offer varying degrees of returns over time – including negative returns - which may be deducted from the current balance of the Investment Amount; and" },
                        new() { Content = "the Contract Holder, Beneficiary and the Funeral Director agree to adhere to the Contract’s Terms of Agreement and applicable laws and regulations at all times." }
                    ]
                },
            ]
        };
    }

    public class ConditionModel
    {
        public string Content { get; set; }

        public List<ConditionModel> Conditions { get; set; }
    }
}
