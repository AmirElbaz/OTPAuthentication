using Microsoft.EntityFrameworkCore;
using OTPService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTPService.DataAccess
{
    public class OTPContext: DbContext
    {
        public OTPContext(DbContextOptions<OTPContext> options) : base(options)
        {
        }

        public DbSet<OTPRecord> records  {  get; set; }
        public DbSet<UserBlock> UserBlocks { get; set; }

    }
}
