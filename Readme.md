# How to implement drag and drop between a scheduler and a grid


<p>This example illustrates how to implement drag and drop for appointments between a SchedulerControl and a GridControl. Note that we cannot use the approach from the <a href="https://www.devexpress.com/Support/Center/p/E3808">How to implement drag and drop between a scheduler and a grid</a> code example for this purpose because the Silverlight platform does not have the <strong>DragDrop.DoDragDrop()</strong> method and corresponding drag and drop events are not raised. Generally speaking, the Silverlight platform has very limited Drag and Drop capabilities. So, we are forced to use custom-made solutions. In this particular example, we reuse a separate DragAndDrop (DnD) library which was written by Azret: <a href="http://community.devexpress.com/blogs/theonewith/archive/2010/04/23/silverlight-drag-and-drop-v2010-vol-1.aspx">Silverlight Drag and Drop – v2010 vol 1</a>.</p>

<br/>


