## Where does it run
On Windows, it is a .NET (4.6.1) app.

## What it is made up of
A command line tool named CsvMapper.exe. Run it from CMD or PowerShell to learn how to to invoke it.
A UI app named CsvMapperUI.exe that simplifies editing the mapping and runs the tool.

## What does it do?
The Csv Mapper transforms a .csv file.

It leaves the Source file unchanged and creates a new file.

It can rename and merge columns as well as merge rows based on a field that contains a unique identifier.

When merging columns, the text is appended in order and separated by "\r\r" (0A 0A).

A typical row merging scenario is when the .csv file contains data from a query that returns the comments for a given record and where each row is identical except for comment field:

    TicketID,Title,Description,Status,Comment
    1,widget fail,Widget fails to work,In Progress,Comment 1
    1,widget fail,Widget fails to work,In Progress,Comment 2
    1,widget fail,Widget fails to work,In Progress,Comment 3
    2,widget2 fail,Widget2 fails to work,In Progress,Widget2 Comment 1
    2,widget2 fail,Widget2 fails to work,In Progress,Widget2Comment 2

In the above scenario, using column 1 as the unique ID, the three rows With Ticket ID 1 will be merged into one row with all three comments appended in the comment field.

## Configuring the mapping
The mapping is done in a simple xml file.

contains a number of tags with a name attribute which determines the name of the column in the target file. You can use the same name as the original or provide a new name.

contains one or more tags that identify the where to get the content of the target field. Specifying more than on tags is how you combine the data of multiple coluns into one.

Finally, if you want to merge rows as described earlier, specify the Target column name that contains the unique ID on which to operate.

Below is an example where we extracted data from Autotask for an import into a different ticket system.

    <?xml version="1.0" encoding="utf-8"?>
    <CsvMap merge_on="Autotask Ticket Number">
      <Target name="Customer">
        <Source>Client name</Source>
      </Target>
      <Target name="Product Line">
        <Source>Product</Source>
      </Target>
      <Target name="AutotaskReporter">
        <Source>Ticket Created By</Source>
      </Target>
      <Target name="AutotaskAssignee">
        <Source>Assigned to Resource name</Source>
      </Target>
      <Target name="Created">
        <Source>Ticket Created Date</Source>
      </Target>
      <Target name="Completed">
        <Source>Ticket Completed Date</Source>
      </Target>
      <Target name="Autotask Ticket Number">
        <Source>Ticket Number</Source>
      </Target>
      <Target name="Summary">
        <Source>Ticket Title</Source>
      </Target>
      <Target name="Description">
        <Source>Ticket Description</Source>
      </Target>
      <Target name="Issue Type">
        <Source>Ticket Type</Source>
      </Target>
      <Target name="Status">
        <Source>Ticket Status</Source>
      </Target>
      <Target name="Note">
        <Source>Note Created By</Source>
        <Source>Note Date Created</Source>
        <Source>Note Detail</Source>
      </Target>
    </CsvMap>
