using System.Linq;
using Orchard.DisplayManagement;
using System.Collections.Generic;
using Wkong.SchedulingTask.Services;
using Orchard.Navigation;
using System;
using Microsoft.AspNetCore.Mvc;
using Wkong.SchedulingTask.ViewModels;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Modules;
using Orchard.Environment.Extensions.Features.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Orchard.Settings;
using System.Threading.Tasks;
namespace Wkong.SchedulingTask.Controllers
{
    [OrchardFeature("Wkong.SchedulingTask.UI")]
    public class AdminController : Controller {
        private readonly ISchedulingTaskManager _schedulingTaskManager;
        //  private readonly IOrchardServices _services;
        private readonly IClock _clock;
        private readonly ISchedulingTaskProcessor _SchedulingTaskProcessor;
        private readonly ISiteService _siteService;
        private readonly ISchedulingTaskService _schedulingTaskService;
      //  private readonly IFormManager _formManager;
      //  private readonly IDateLocalizationServices _dateLocalizationServices;
        public AdminController(
            ISchedulingTaskManager schedulingTaskManager,
             ISiteService siteService,
             IClock clock,
            IShapeFactory shapeFactory,
            //IOrchardServices services, 
            ISchedulingTaskService schedulingTaskService,
            //IFormManager formManager,
          //  IDateLocalizationServices dataLocalizationServices,
             IHtmlLocalizer<AdminController> t,
            ISchedulingTaskProcessor schedulingTaskProcessor) {
            _clock = clock;
            _schedulingTaskManager = schedulingTaskManager;
          //  _services = services;
            _SchedulingTaskProcessor = schedulingTaskProcessor;
            _schedulingTaskService = schedulingTaskService;
            New = shapeFactory;
            T = t;
            _siteService = siteService;
         //   _dateLocalizationServices = dataLocalizationServices;
          //  _formManager = formManager;
        }

        public dynamic New { get; set; }
        public IHtmlLocalizer T { get; set; }

        public async Task<IActionResult> List(PagerParameters pagerParameters) {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            Pager pager = new Pager(pagerParameters, siteSettings.PageSize);

            var jobsCount = _schedulingTaskManager.GetTasksCount();
            var jobs = _schedulingTaskManager.GetAllTask(pager.GetStartIndex(), pager.PageSize).ToList();
            var model = New.ViewModel()
                .Pager(New.Pager(pager).TotalItemCount(jobsCount))
                .Jobs(jobs)
                ;

            return View(model);
        }

        public ActionResult AddTask()
        {          
            var allTasks= _schedulingTaskManager.GetSchedulingTasks();
            var model = new SchedulingTaskViewModel()
            {
                SchedulingTasks = allTasks.Select(x => new SchedulingTaskEntry()
                {
                    Category = x.Category.Value,
                    Description = x.Description.Value
                    ,
                    MessageName = x.MessageName,
                    Selected = false,
                    TaskName = x.Name
                }).ToList(),

            };
            return View(model);
            
        }
         [HttpPost, ActionName("AddTask")]
        public ActionResult AddTask(SchedulingTaskViewModel model )
         {
             var selectedTask = model.SchedulingTasks.FirstOrDefault(x=>x.Selected);
             if (selectedTask != null)
             {
                var utcDateTime = DateTime.Parse(model.Date+" "+model.Time);// _dateLocalizationServices.ConvertFromLocalizedString(model.Date, model.Time);
                 _schedulingTaskService.EnqueueAsync(model.TaskName, selectedTask.MessageName, model.Priority, utcDateTime, model.Frequency, model.SpaceNum);
               
             }
             return RedirectToAction("List");
         }
         public ActionResult PauseTask(int Id)
         {
             var task = _schedulingTaskManager.GetTask(Id);
             task.CanExecute = false;
             _schedulingTaskService.EditTaskAsync(task);
             return RedirectToAction("List");

         }
         public ActionResult ResumeTask(int Id)
         {
             var task = _schedulingTaskManager.GetTask(Id);
             task.CanExecute = true;             
             _schedulingTaskService.EditTaskAsync(task);
             return RedirectToAction("List");

         }
         public ActionResult EditTask(int Id)
         {
             var task = _schedulingTaskManager.GetTask(Id);
             var allTasks = _schedulingTaskManager.GetSchedulingTasks();
             var model = new SchedulingTaskViewModel()
             {
                 SchedulingTasks = allTasks.Select(x => new SchedulingTaskEntry()
                 {
                     Category = x.Category.Value,
                     Description = x.Description.Value
                     ,
                     MessageName = x.MessageName,
                     Selected = false,
                     TaskName = x.Name
                 }).ToList(),

             };
             model.Id = task.Id;
             model.Priority = task.Priority;
             model.SpaceNum = task.SpaceNum;
             model.TaskName = task.TaskName;
             model.Frequency = task.Frequency;
            model.Date = task.ScheduledUtc.Date.ToString();// _dateLocalizationServices.ConvertToLocalizedDateString(task.ScheduledUtc);
            model.Time = task.ScheduledUtc.TimeOfDay.ToString();// _dateLocalizationServices.ConvertToLocalizedTimeString(task.ScheduledUtc);
             return View(model);

         }
         [HttpPost, ActionName("EditTask")]
         public ActionResult EditTask(SchedulingTaskViewModel model)
         {
             var task = _schedulingTaskManager.GetTask(model.Id);
             var utcDateTime = DateTime.Parse(model.Date + " " + model.Time); //_dateLocalizationServices.ConvertFromLocalizedString(model.Date, model.Time);

             //task.ScheduledUtc = utcDateTime.Value;
             task.Priority = model.Priority;
             task.SpaceNum = model.SpaceNum;
             task.TaskName = model.TaskName;
             task.Frequency = model.Frequency;
             _schedulingTaskService.EditTaskAsync(task);
             return RedirectToAction("List");
         }
       /*  public ActionResult EditTaskParameters(int id)
         {

             var model = _schedulingTaskManager.GetTask(id);

             var task = _schedulingTaskManager.GetSchedulingTaskByMessageName(model.Message);

             if (task == null)
             {
                return new NotFoundResult();// HttpNotFound();
             }
             var form = task.Form == null ? null : _formManager.Build(task.Form);
             if (!string.IsNullOrEmpty(model.Parameters))
             {
                 var parameters =model.Parameters.ToDic();//FormParametersHelper.FromJsonString();
                 var para1=(IDictionary<string, object>)parameters["parameters"];
                 var par =new Dictionary<string,string>(); //;
                 foreach(string key in para1.Keys)
                 {
                     par.Add(key,Convert.ToString(para1[key]));
                 }

                 _formManager.Bind(form, new DictionaryValueProvider<string>(par, System.Globalization.CultureInfo.InvariantCulture));
             }
             var viewModel = New.ViewModel(Id: id, Form: form, State: model.Parameters);

             return View(viewModel);
         }
         [HttpPost, ActionName("EditTaskParameters")]
         //[FormValueRequired("_submit.Save")]
         public ActionResult EditTaskParameters(int Id,  FormCollection formValues)
         {
             var name=_schedulingTaskManager.GetTask(Id).Message;
             var task = _schedulingTaskManager.GetSchedulingTaskByMessageName(name);

             if (task == null)
             {
                return new NotFoundResult();//HttpNotFound();
             }

             _formManager.Validate(new ValidatingContext { FormName = task.Form, ModelState = ModelState, ValueProvider = ValueProvider });

             if (!ModelState.IsValid)
             {

                 var form = task.Form == null ? null : _formManager.Build(task.Form);

                 _formManager.Bind(form, ValueProvider);
                 var viewModel = New.ViewModel(Id: Id, Form: form);

                 return View(viewModel);
             }
              string  value = FormParametersHelper.ToJsonString(formValues);
              _schedulingTaskService.EditTaskAsync(Id, JsonConvert.DeserializeObject(value));
             return RedirectToAction("List" );
         }*/

    }
}