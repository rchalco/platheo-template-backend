using FluentResults;
using VectorStinger.Core.Domain.DataBase.Data;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterTemplateWebHandle;
using VectorStinger.Core.Interfaces.Managers.WebTemplate;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Infrastructure.DataAccess.Interface;

namespace VectorSinger.Modules.WebTemplate.Mangers
{
    public class WebTemplateManager : BaseManager<DbearthBnbContext>, IWebTemplateManager
    {
       
        public WebTemplateManager(IRepository repository)
            : base(repository)
        {
           
        }

        public Result<ResoponseRegisterTemplateWebDTO> RegisterTemplateWebHandle(RequestRegisterTemplateWebDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
