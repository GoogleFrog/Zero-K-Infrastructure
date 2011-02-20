﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UnlocksAdmin.aspx.cs" Inherits="ZeroKWeb.UnlocksAdmin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
      <asp:GridView ID="GridView1" runat="server" AllowPaging="True" 
        AllowSorting="True" AutoGenerateColumns="False" AutoGenerateDeleteButton="True" 
        AutoGenerateEditButton="True" DataKeyNames="UnlockID" PageSize="40"
        DataSourceID="LinqDataSource1">
        <Columns>
        <asp:BoundField DataField="UnlockID" HeaderText="ID" 
            SortExpression="UnlockID" />
        <asp:BoundField DataField="Code" HeaderText="Code" 
            SortExpression="Code" />
          <asp:BoundField DataField="Name" HeaderText="Name" 
            SortExpression="Name" />
          <asp:BoundField DataField="XpCost" HeaderText="XpCost" SortExpression="XpCost" />
          <asp:BoundField DataField="NeededLevel" HeaderText="Level" 
             SortExpression="NeededLevel" />
          <asp:BoundField DataField="RequiredUnlockID" HeaderText="RequiredID" 
             SortExpression="RequiredUnlockID" />
          <asp:BoundField DataField="UnlockType" HeaderText="Type" 
            SortExpression="UnlockType" />
            <asp:BoundField DataField="MorphLevel" HeaderText="Morph" 
             SortExpression="MorphLevel" />
             <asp:BoundField DataField="MaxModuleCount" HeaderText="MaxCount" 
             SortExpression="MaxModuleCount" />
          <asp:BoundField DataField="LimitForChassis" HeaderText="LimitChassis" 
             SortExpression="LimitForChassis" />
          <asp:BoundField DataField="MetalCost" HeaderText="Metal" SortExpression="MetalCost" />
          <asp:BoundField DataField="MetalCostMorph2" HeaderText="Metal2" SortExpression="MetalCostMorph2" />
          <asp:BoundField DataField="MetalCostMorph3" HeaderText="Metal3" SortExpression="MetalCostMorph3" />
          <asp:BoundField DataField="MetalCostMorph4" HeaderText="Metal4" SortExpression="MetalCostMorph4" />
          <asp:BoundField DataField="Description" HeaderText="Description" 
             SortExpression="Description" />
        </Columns>
      </asp:GridView>
      <asp:LinqDataSource ID="LinqDataSource1" runat="server" 
        ContextTypeName="ZkData.ZkDataContext" EnableDelete="True" EnableInsert="True" 
        EnableUpdate="True" EntityTypeName="" 
        TableName="Unlocks">
      </asp:LinqDataSource>
    </div>

    <h3>Insert new</h3>
    Code: <asp:TextBox ID="tbCode" runat="server"></asp:TextBox><br />
    Name: <asp:TextBox ID="tbName" runat="server"></asp:TextBox><br />
    XpCost: <asp:TextBox ID="tbXpCost" runat="server" Text="200"></asp:TextBox><br />
    Needed level: <asp:TextBox ID="tbMinLevel" runat="server" Text="0"></asp:TextBox><br />
    Prerequisite (ID): <asp:TextBox ID="tbPreq" runat="server"></asp:TextBox><br />
    Unlock type: <asp:DropDownList ID="ddType" runat="server">
    </asp:DropDownList><br />
    Morph level (when module/weapon is available): <asp:TextBox ID="tbMorphLevel" runat="server"></asp:TextBox><br />
    Max module count (in one commander): <asp:TextBox ID="tbMaxCount" runat="server">1</asp:TextBox><br />
    For chassis (comma list): <asp:TextBox ID="tbChassisLimit" runat="server"></asp:TextBox><br />
    Metal cost (ingame): <asp:TextBox ID="tbMetalCost" runat="server"></asp:TextBox><br />
    Morph2 metal cost(chassis): <asp:TextBox ID="tbMorph2" runat="server"></asp:TextBox><br />
    Morph3 metal cost(chassis): <asp:TextBox ID="tbMorph3" runat="server"></asp:TextBox><br />
    Morph4 metal cost(chassis): <asp:TextBox ID="tbMorph4" runat="server"></asp:TextBox><br />
    Description: <asp:TextBox ID="tbDescription" runat="server" Rows="4" Columns="40"></asp:TextBox><br />
    <asp:Button ID="btnAdd" runat="server" Text="Add new" onclick="btnAdd_Click" />
    </form>
</body>
</html>
