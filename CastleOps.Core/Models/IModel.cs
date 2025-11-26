using System;
using System.Collections.Generic;
using System.Text;

namespace CastleOps.Core.Models
{
    public interface IModel
    {
        Guid Id { get; set; }
        DateTime DateCreated { get; set; }
    }
}