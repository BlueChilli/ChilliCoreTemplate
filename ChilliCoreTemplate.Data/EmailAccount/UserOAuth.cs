using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChilliCoreTemplate.Models;
using ChilliCoreTemplate.Models.Api.OAuth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Data.EmailAccount
{
    public class UserOAuth
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public OAuthProvider Provider { get; set; }

        [Required, MaxLength(50)]
        public string OAuthId { get { return _OAuthId; } set { _OAuthId = value; OAuthIdHash = CommonLibrary.CalculateHash(value).Value; } }
        private string _OAuthId;
        public int OAuthIdHash { get; set; }

        [MaxLength(1000)]
        public string Token { get; set; }

    }

    public class UserOAuthConfiguration : IEntityTypeConfiguration<UserOAuth>
    {
        public void Configure(EntityTypeBuilder<UserOAuth> builder)
        {
            builder.HasIndex(x => x.OAuthIdHash);
        }
    }

}
