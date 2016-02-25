using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace pCOLADnamespace
{
    public class ImageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WithImage
        { get; set; }
        public DataTemplate WithoutImage
        { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //pSHARE pS = item as pSHARE;
            DataRowView drv = item as DataRowView;
            if (drv != null)
            {
                if (pSHARE.historyOn)
                {
                    return WithoutImage;
                }
                else
                {
                    return WithImage;
                }
            }
            else
            {
                return base.SelectTemplate(item, container);
            }
        }

    }
}

