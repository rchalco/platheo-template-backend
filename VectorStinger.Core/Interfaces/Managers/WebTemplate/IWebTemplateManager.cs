using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterTemplateWebHandle;

namespace VectorStinger.Core.Interfaces.Managers.WebTemplate
{
    public interface IWebTemplateManager
    {
        Result<ResponseRegisterTemplateWebDTO> RegisterTemplateWebHandle(RequestRegisterTemplateWebDTO request);

    }
}
