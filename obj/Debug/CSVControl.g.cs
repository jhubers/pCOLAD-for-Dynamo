﻿#pragma checksum "..\..\CSVControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B1E70BFA8B8C6FF7C1F39E845EA89C54"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Windows.Themes;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using pCOLADnamespace;


namespace pCOLADnamespace {
    
    
    /// <summary>
    /// CSVControl
    /// </summary>
    public partial class CSVControl : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 8 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal pCOLADnamespace.CSVControl pCOLADwindow;
        
        #line default
        #line hidden
        
        /// <summary>
        /// myXamlTable Name Field
        /// </summary>
        
        #line 241 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public System.Windows.Controls.DataGrid myXamlTable;
        
        #line default
        #line hidden
        
        
        #line 282 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGridTemplateColumn dgtc;
        
        #line default
        #line hidden
        
        
        #line 409 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Cancel;
        
        #line default
        #line hidden
        
        
        #line 412 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Share;
        
        #line default
        #line hidden
        
        
        #line 415 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button UnCheckAll;
        
        #line default
        #line hidden
        
        
        #line 418 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CheckAll;
        
        #line default
        #line hidden
        
        
        #line 423 "..\..\CSVControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton History;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/pCOLAD;component/csvcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\CSVControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.pCOLADwindow = ((pCOLADnamespace.CSVControl)(target));
            
            #line 16 "..\..\CSVControl.xaml"
            this.pCOLADwindow.Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            
            #line 16 "..\..\CSVControl.xaml"
            this.pCOLADwindow.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.pCOLADwindow_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.myXamlTable = ((System.Windows.Controls.DataGrid)(target));
            
            #line 241 "..\..\CSVControl.xaml"
            this.myXamlTable.AutoGeneratingColumn += new System.EventHandler<System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs>(this.myXamlTable_AutoGeneratingColumn);
            
            #line default
            #line hidden
            
            #line 242 "..\..\CSVControl.xaml"
            this.myXamlTable.AutoGeneratedColumns += new System.EventHandler(this.myXamlTable_AutoGeneratedColumns);
            
            #line default
            #line hidden
            return;
            case 8:
            this.dgtc = ((System.Windows.Controls.DataGridTemplateColumn)(target));
            return;
            case 9:
            this.Cancel = ((System.Windows.Controls.Button)(target));
            return;
            case 10:
            this.Share = ((System.Windows.Controls.Button)(target));
            return;
            case 11:
            this.UnCheckAll = ((System.Windows.Controls.Button)(target));
            
            #line 416 "..\..\CSVControl.xaml"
            this.UnCheckAll.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.CheckColor);
            
            #line default
            #line hidden
            return;
            case 12:
            this.CheckAll = ((System.Windows.Controls.Button)(target));
            
            #line 419 "..\..\CSVControl.xaml"
            this.CheckAll.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.CheckColor);
            
            #line default
            #line hidden
            return;
            case 13:
            this.History = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 2:
            
            #line 143 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.TextBlock)(target)).MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.CommentTextBlock_MouseUp);
            
            #line default
            #line hidden
            break;
            case 3:
            
            #line 145 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.TextBox)(target)).LostFocus += new System.Windows.RoutedEventHandler(this.TextBox_LostFocus);
            
            #line default
            #line hidden
            
            #line 146 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.TextBox)(target)).PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.CommentTextBox_PreviewTextInput);
            
            #line default
            #line hidden
            break;
            case 4:
            
            #line 200 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.Border)(target)).MouseLeave += new System.Windows.Input.MouseEventHandler(this.imageBorder_MouseLeave);
            
            #line default
            #line hidden
            
            #line 201 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.Border)(target)).MouseEnter += new System.Windows.Input.MouseEventHandler(this.imageBorder_MouseEnter);
            
            #line default
            #line hidden
            break;
            case 5:
            
            #line 204 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.Image)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.myImage_MouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 204 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.Image)(target)).MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.myImage_MouseRightButtonUp);
            
            #line default
            #line hidden
            break;
            case 7:
            
            #line 252 "..\..\CSVControl.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Loaded += new System.Windows.RoutedEventHandler(this.myCheckBox_Loaded);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

