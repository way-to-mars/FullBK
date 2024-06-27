using FullBK.Model;
using FullBK.ViewModel;
using System.Diagnostics;

namespace FullBK.View;

public partial class GroupingCollectionView : ContentPage
{
    // Expected width of a single ItemView including paddings
    private readonly double itemRoomWidth = 220;
    int columnsCount = 2;

    public GroupingCollectionView(GroupingCollectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        int count = (int)(width / itemRoomWidth);
        if (count < 2) count = 2;
        
        if (count != columnsCount)
        {
            columnsCount = count;

            var cv = (CollectionView)Content;
            if (cv!=null) ((GridItemsLayout)cv.ItemsLayout).Span = count;
        }

        base.OnSizeAllocated(width, height);
    }

}