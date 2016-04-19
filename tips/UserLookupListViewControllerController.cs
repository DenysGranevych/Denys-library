using System;
using DevExpress.Data.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Web.ASPxTreeList;
using DevExpress.Persistent.Base.General;
using DevExpress.ExpressApp.TreeListEditors.Web;
using System.Web.UI.WebControls;

namespace SWEPromoTool.Module.Web.Controllers
{
    public partial class UserLookupListViewControllerController : ObjectViewController<ListView, ITreeNode>
    {
        ASPxTreeList treeList;
        ParametrizedAction findNodeAction;
        public UserLookupListViewControllerController()
        {
            //findNodeAction = new ParametrizedAction(this, "FindNode", DevExpress.Persistent.Base.PredefinedCategory.FullTextSearch, typeof(String));
            //findNodeAction.Caption = "Smart Search";
            //findNodeAction.PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.Image;
            //findNodeAction.Execute += new ParametrizedActionExecuteEventHandler(findNodeAction_Execute);
        }
        protected override void OnViewControlsCreated() {
            base.OnViewControlsCreated();
            ASPxTreeListEditor treeListEditor = View.Editor as ASPxTreeListEditor;
            if (treeListEditor != null) {
                treeList = treeListEditor.TreeList;
                treeList.HtmlDataCellPrepared += treeList_HtmlDataCellPrepared;
            }
        }
        void treeList_HtmlDataCellPrepared(object sender, TreeListHtmlDataCellEventArgs e)
        {
            var temp = e.Level;
            string textToSearch = "s";//findNodeAction.Value as String;
            if (!String.IsNullOrEmpty(textToSearch) && e.CellValue != null) {
                string propertyValue = e.CellValue.ToString();
                int textIndex = propertyValue.ToLower().IndexOf(textToSearch.ToLower());
                if (textIndex >= 0) {
                    int spanLength = ("<span class='highlight'>").Length;
                    propertyValue = propertyValue.Insert(textIndex, "<span class='highlight'>");
                    propertyValue = propertyValue.Insert(textIndex + spanLength + textToSearch.Length, "</span>");
                    Label label = new Label();
                    label.Text = propertyValue;
                    e.Cell.Controls.Clear();
                    e.Cell.Controls.Add(label);
                }
            }
        }
        //void findNodeAction_Execute(object sender, ParametrizedActionExecuteEventArgs e) {
        //    string searchText = e.ParameterCurrentValue as String;
        //    if (!String.IsNullOrEmpty(searchText)) {
        //        treeList.ExpandAll();
        //    }
        //}

        protected override void OnDeactivated() {
            base.OnDeactivated();
            if (treeList != null) {
                treeList.HtmlDataCellPrepared -= treeList_HtmlDataCellPrepared;
                treeList = null;
            }
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.Data.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.TreeListEditors.Web;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Web.ASPxTreeList;
using DevExpress.Xpo;
using SWEPromoTool.Module.BusinessObjects.Enums;
using SWEPromoTool.Module.BusinessObjects.PT;

namespace SWEPromoTool.Module.Web.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class UserLookupListViewController : ViewController
    {
        public UserLookupListViewController()
        {
            InitializeComponent();
            RegisterActions(components);
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            View.ControlsCreated -= View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {

            //var treeList = ((ASPxTreeListEditor)((ListView)View).Editor).TreeList.VirtualModeNodeCreating += NodeCreating;
            //    var sdfsd = treeList.DataSource;

            //    XPCollection<User> source = new XPCollection<User>(new Session(),
            //CriteriaOperator.Parse("Text = 'A21' or Text = 'B1'"));
            //    List<User> parents = new List<User>();
            //    foreach (User rec in source)
            //    {
            //        if (rec.Parent != null && !parents.Contains(rec.Parent))
            //            parents.AddRange(GetParents(rec));
            //    }
            //    source.AddRange(parents);
            //    treeList.DataSource = source;
            //    treeList.DataBind();





            //    var temp2 = treeList.Columns["Статус"] as TreeListDataColumn;
            //    var temp3 = ((ASPxTreeListEditor)((ListView)View).Editor);
            //    treeList.GetAllNodes().ToList().ForEach(x =>
            //    {
            //        var temp = x.DataItem;
            //    });
            //    //((ASPxTreeListEditor)((ListView)View).Editor).TreeList.DeleteNode();
            ((ASPxTreeListEditor)((ListView)View).Editor).TreeList.VirtualModeCreateChildren += NodeCreatingChildren;
            ((ASPxTreeListEditor)((ListView)View).Editor).TreeList.VirtualModeNodeCreating += NodeCreating;
            ((ASPxTreeListEditor)((ListView)View).Editor).TreeList.PreRender += new EventHandler(TreeList_PreRender);
        }

        private void NodeCreating(object sender, TreeListVirtualModeNodeCreatingEventArgs e)
        {
            var user = e.NodeObject as User;
        }

        private void NodeCreatingChildren(object sender, TreeListVirtualModeCreateChildrenEventArgs e)
        {
            var removedNodes = new List<object>();
            foreach (var node in e.Children)
            {
                if ((node as User).LevelValue == LevelOrgStructureEnum.RKAMOrKAM)
                {
                    removedNodes.Add(node);
                }
            }

            foreach (var removedNode in removedNodes)
            {
                e.Children.Remove(removedNode);
            }
        }


        void TreeList_PreRender(object sender, EventArgs e)
        {
            var treeList = ((ASPxTreeList)sender);
            //treeList.DeleteNode();
            treeList.GetAllNodes().ToList().ForEach(x =>
           {
               var temp = x.DataItem;
               var nodeDictanori = temp as Dictionary<string, object>;
               var user = nodeDictanori["__RowObjectColumn"] as User;
               if (user.LevelValue == LevelOrgStructureEnum.RKAMOrKAM)
               {
                   treeList.DeleteNode(x.Key);
               }
               var test = x.ChildNodes;
               var s = 5;
           });
        }

        private List<User> GetParents(User source)
        {
            if (null == source.Parent) throw new ArgumentException("The source has no parent", "source");
            List<User> result = new List<User>() { source.Parent };
            if (source.Parent.Parent != null) result.AddRange(GetParents(source.Parent));
            return result;
        }
    }
}
