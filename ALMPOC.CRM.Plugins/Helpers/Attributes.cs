using System;
using ALMPOC.CRM.Plugins.Helpers;

/// <summary>
/// Use this attribute to specify the registration information in MS CRM. You can use this in conjunction with the "ValidateExecutionContext"-method to automatically validate the execution context
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RegistrationInfo : Attribute
{
    /// <summary>
    /// Messagename (e.g. MessageName.Create) on which the plugin needs to trigger
    /// </summary>
    public string MessageName { get; set; }
    /// <summary>
    /// Stage (e.g. MessageProcessingStage.AfterMainOperationInsideTransaction) on which the plugin needs to trigger
    /// </summary>
    public int Stage { get; set; }
    /// <summary>
    /// Mode (e.g. MessageMode.Synchronous) to indicate how the plugin class will be processed.
    /// </summary>
    public int Mode { get; set; }
    /// <summary>
    /// Name of the Post Entity Image (e.g. Target). You can specify the attributes which you want to include in the image, seperated by a ";" e.g. : Target|name;parentcustomerid
    /// </summary>
    public string PostEntityImageName { get; set; }
    /// <summary>
    /// Name of the Pre Entity Image (e.g. Target). You can specify the attributes which you want to include in the image, seperated by a ";" e.g. : Target|name;parentcustomerid
    /// </summary>
    public string PreEntityImageName { get; set; }
    /// <summary>
    /// Name of the entity (e.g. account) on which the plugin needs to trigger
    /// </summary>
    public string PrimaryEntityName { get; set; }
    /// <summary>
    /// Specify the attribute filters on which you wish to trigger the plugin. You can specify the pipe symbol ";" as a seperator, e.g. name;parentcustomerid
    /// </summary>
    public string FilteringAttributes { get; set; }
    /// <summary>
    /// Specify the execution order in which you wish to trigger the plugin.
    /// </summary>
    public int ExecutionOrder { get; set; }

    /// <summary>
    /// Returns a string containing all specified registration attributes
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string registrationInfo = string.Empty;

        if (!String.IsNullOrEmpty(this.MessageName))
        {
            registrationInfo += " MessageName:" + this.MessageName;
        }
        //if (this.Mode != int.MinValue)
        //{
        registrationInfo += " Mode:" + this.Mode;
        //}
        if (this.Stage != 0)
        {
            registrationInfo += " Stage:" + this.Stage;
        }
        if (!String.IsNullOrEmpty(this.PreEntityImageName))
        {
            registrationInfo += " PreEntityImageName:" + this.PreEntityImageName;
        }
        if (!String.IsNullOrEmpty(this.PostEntityImageName))
        {
            registrationInfo += " PostEntityImageName:" + this.PostEntityImageName;
        }
        if (!String.IsNullOrEmpty(this.PrimaryEntityName))
        {
            registrationInfo += " PrimaryEntityName:" + this.PrimaryEntityName;
        }

        return registrationInfo;

    }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class CrmAssemblyInfo : Attribute
{
    public string SolutionUniqueName { get; set; }
    public int AssemblySourceType { get; set; }
    public int IsolationMode { get; set; }

}

