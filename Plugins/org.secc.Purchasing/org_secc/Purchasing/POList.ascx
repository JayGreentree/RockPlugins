﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="POList.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Purchasing.POList" %>
<%@ Register Src="~/Plugins/org_secc/Purchasing/StaffPicker.ascx" TagPrefix="secc" TagName="StaffPicker" %>

<asp:UpdatePanel ID="upMain" runat="server" class="panel panel-block">
    <ContentTemplate>
        <div class="panel-heading">
            <h1 class="panel-title"><i class="fa fa-columns"></i>&nbsp;Purchase Order List</h1>
            <asp:LinkButton ID="lbAddPO" runat="server" OnClick="lbAddPO_Click" class="btn-add btn btn-default btn-sm pull-right"><i class="fa fa-plus"></i>&nbsp;&nbsp;New Purchase Order</asp:LinkButton>
        </div>
        <div class="panel-body">
            <div class="grid grid-panel">
                <Rock:ModalAlert runat="server" ID="nbAlert" />
                <Rock:GridFilter runat="server" ID="gfPurchaseOrders" OnApplyFilterClick="btnFilterApply_Click" OnClearFilterClick="btnFilterClear_Click">
                    <asp:Panel runat="server">
                        <label>Status</label><br />
                        <a onclick="$('input[id*=\'cbListStatus\'][type=checkbox]').prop('checked', true); return false;" href="#">Check All</a>&nbsp;
                        <a onclick="$('input[id*=\'cbListStatus\'][type=checkbox]').prop('checked', false); return false;" href="#">Uncheck All</a>
                        <Rock:RockCheckBoxList ID="cbListStatus" runat="server" RepeatColumns="5" RepeatDirection="horizontal"
                            CssClass="smallText" TextAlign="Right" />
                    </asp:Panel>
                    <Rock:RockCheckBoxList Label="Type:" ID="cbListType" runat="server" RepeatDirection="Horizontal" TextAlign="Right" />
                    
                    <asp:Panel runat="server">
                        <div class="row">
                            <div class="col-xs-4">
                                <Rock:RockCheckBox Label="Active/Inactive:" ID="chkShowInactive" runat="server" Text="Show Inactive" />
                            </div>
                            <div class="col-xs-8">
                                <div class="form-group">
                                    <label class="form-label">Ordered By:</label>
                                    <secc:StaffPicker ID="ucStaffPicker" runat="server" AllowMultipleSelections="false" ShowPersonDetailLink="false" UserCanEdit="true" DefaultLabel="Show All" />
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                    <Rock:RockDropDownList Label="Vendor:" ID="ddlVendor" runat="server" CssClass="smallText" />
                    <Rock:RockTextBox Label="PO Number:" id="txtPONumber" runat="server" />
                    
                    <asp:Panel runat="server" CssClass="form-group" ID="pnlDateRange">
                        <label>Order Date</label><br />
                        <Rock:DateRangePicker ID="txtOrderDate" runat="server" style="float: left" />
                        <button class="btn btn-default" onclick="$(this).siblings().find('input').val('')"><i class="fa fa-remove"></i></button>
                    </asp:Panel>

                    <Rock:RockTextBox ID="tbGLAccount" runat="server" Label="General Ledger Account"></Rock:RockTextBox>
                    
                    <Rock:RockDropDownList Label="Payment Method:" ID="ddlPaymentMethod" runat="server" />
                    <asp:Panel runat="server" CssClass="form-group" ID="pnlPaymentDate">
                        <label>Payment Date</label><br />
                        <Rock:DateRangePicker ID="drpPaymentDate" runat="server" style="float: left" />
                        <button class="btn btn-default" onclick="$(this).siblings().find('input').val('')"><i class="fa fa-remove"></i></button>
                    </asp:Panel>
                    
                </Rock:GridFilter>
                <Rock:Grid ID="dgPurchaseOrders" runat="server" CssClass="list" 
                    OnReBind="dgPurchaseOrders_Rebind" OnItemCommand="dgPurchaseOrders_ItemCommand" >
                    <Columns>
                       <Rock:RockBoundField HeaderText="PurchaseOrderID" DataField="PurchaseOrderID" Visible="false" />
                       <Rock:RockTemplateField HeaderText="PO Number" SortExpression="PurchaseOrderID" >     
                            <ItemTemplate>
                                <asp:HyperLink runat="server" text='<%# Eval("PurchaseOrderID") %>' NavigateUrl='<%# String.Format("{0}?poid={1}", PurchaseOrderDetailPageSetting, Eval("PurchaseOrderID")) %>'></asp:HyperLink> 
                            </ItemTemplate>
                       </Rock:RockTemplateField>
                       <Rock:RockBoundField HeaderText="Vendor" DataField="VendorName" SortExpression="VendorName" />
                       <Rock:RockBoundField HeaderText="Type" DataField="POType" SortExpression="POType" />
                       <Rock:RockBoundField HeaderText="Status" DataField="Status" SortExpression="Status" />
                       <Rock:RockBoundField HeaderText="Items Details" DataField="ItemDetails" SortExpression="ItemDetailCount" />
                       <Rock:RockBoundField HeaderText="Total Payments" DataField="TotalPayments" SortExpression="TotalPayments" />
                       <Rock:RockBoundField HeaderText="Notes" DataField="NoteCount" SortExpression="NoteCount" />
                       <Rock:RockBoundField HeaderText="Attachments" DataField="AttachmentCount" SortExpression="AttachmentCount" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>
        <script type="text/javascript">
            // Expand the filters area
            var expandFilters = function ()
            {
                el = $('.grid-filter header').get();
                $('i.toggle-filter', el).toggleClass('fa-chevron-down fa-chevron-up');
                $(el).siblings('div').slideDown(0);
            }
            $(document).ready(expandFilters);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(expandFilters);
        </script>
        <style>
            .grid-filter h4 {
                display: none;
            }
            .grid-filter header {
                display: none;
            }
        </style>
    </ContentTemplate>
</asp:UpdatePanel>