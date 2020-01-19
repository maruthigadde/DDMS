$siteUrl = "https://infyakash.sharepoint.com/sites/ddmspoc"
$documentLib = "DDMSData"
$baseFileName = $documentLib

#startingRecord value starts with 1 and can have multple of $pageSize value plus 1. For e.g. 1, 101, 201....
$startingRecord = 1

#endingRecord value always have multiple of $pageSize and the value should be greater than $startingRecord 
$endingRecord = 100000

$CurrentLocation = (Get-Location).Path


#Maximum number of items can retrieve from list at a time3
$pageSize = 500

#Maximum number of items can copy to one report. This value should be multiple of #pageSize. e.g. 20X100 = 2000
$Global:maxItemsPerCsv = 20000

#file and folder specific variables
$date = Get-Date
$folder = "" + $date.Year +"\" + $date.Month + "\" + $date.Day
$ReportLocation = $CurrentLocation + "\" + $folder
$logFileName = $CurrentLocation + "\" + $folder + "\Log" + "_"+ $date.Hour + "_"+ $date.Minute + ".log"
$Global:fileName = $null

#This method provides report file name based on the interval, which is used to track specific rang of data
function GetFileName($startVal, $endVal)
{
    $date = Get-Date
    $fileName = $ReportLocation +"\" + $folder.Replace("\","_") + "_"+ $startVal + "_"+ $endVal + ".csv"
    return $fileName
}

#This method creates report folder if not exist. The folder structure is: YYYY\MM\DD
function CreateReportFolder()
{
    try
    {
        $null = md $folder -ErrorAction SilentlyContinue
    }
    catch
    {
    }
}

#This method used to write log message to console and a file
function WriteLog($logMessage)
{
   Write-Host $logMessage
   write ((Get-Date).ToLongTimeString() + ": " + $logMessage) | Out-File $logFileName -Append 
}

#Returns credentials object
function GetCredentials()
{
    $username = "hondadevuser@infyakash.onmicrosoft.com"
    $password = "Honda@2019"
    $encpassword = convertto-securestring -String $password -AsPlainText -Force
    $credential = new-object -typename System.Management.Automation.PSCredential -ArgumentList $username, $encpassword
    return $credential
}

#This method creates object with item properties and returns to called method
function GetItemProperties($id, $item)
{
    if($item.FieldValues.DocumentumId -ne $null -or $item.FieldValues.DocumentumVersion -ne $null)
    {
        $obj = New-Object System.Object
    
        $obj | Add-Member -MemberType NoteProperty -Name "SharePoint Item Id" -Value $id
        $fieldValue = $item.FieldValues.UniqueId.ToString()
        $obj | Add-Member -MemberType NoteProperty -Name "SharePoint UniqueId" -Value $fieldValue
        $obj | Add-Member -MemberType NoteProperty -Name "SharePoint Version" -Value $item.FieldValues._UIVersionString
        $obj | Add-Member -MemberType NoteProperty -Name "Title" -Value $item.FieldValues.Title
        $obj | Add-Member -MemberType NoteProperty -Name "DocumentumId" -Value $item.FieldValues.DocumentumId
        $obj | Add-Member -MemberType NoteProperty -Name "DocumentumVersion" -Value $item.FieldValues.DocumentumVersion

        return $obj
    }
    else
    {
        return $null
    }
}

#This methods each item property from the collection along with its version details
function ReadListItems($items)
{
    try
    {
        $i = 1
        $count = $items.Count
        $listData = @()

        foreach($item in $items)
        {
            #Showing progress
            $progress = [int]($i/$count * 100)
            Write-Progress -Activity "Processing $i out of $count" -Status "$progress% Complete:" -PercentComplete $progress -id 2

            $itemId = $item.Id
            WriteLog ("Processing item id: " + $itemId)

            #Getting current version item properties   
            $itemId = $item.Id                     
            $obj = GetItemProperties $itemId $item
            if($obj -ne $null)
            {
                $listData += $obj
            }

            #Getting previous versions data
            $versions = $item.Versions
            $ctx.Load($versions)
            $ctx.ExecuteQuery()

            if($versions.Count -gt 1)
            {
                foreach($version in $versions)  
                {  
                    # Check if it is not current version  
                    if(!$version.IsCurrentVersion)  
                    {  
                        $obj = GetItemProperties $itemId $version
                        if($obj -ne $null)
                        {
                            $listData += $obj
                        }
                    }  
                }
            }

            WriteLog ("Processed item id: " + $itemId)
            $i++
        }

        $listData | Export-Csv -Path $Global:fileName -Append -NoTypeInformation
    }
    catch
    {
        WriteLog ("Error while processing. Exception: " + $_.Exception.Message)
        WriteLog ("Stack trace: " + $_.Exception.StackTrace)
        throw
    }
    finally
    {
        Write-Progress -Activity "Completed!" -Completed $true -id 2
    }
}

#This main method executes all required methods to collect all list item properties along with version data
function Main()
{
    CreateReportFolder
    WriteLog "Report generation started!"

    $credentials = GetCredentials
    
    try
    {
        Connect-PnPOnline $siteUrl -Credentials $credentials
        WriteLog "Connected to $siteUrl"

        $list = Get-PnPList -Identity $documentLib
        if($list -ne $null)
        {        
            $ctx = Get-PnPContext
            $Global:iCnt = 0
            $cnt = $list.ItemCount
            $Global:fileName = GetFileName

            $null = Get-PnPListItem -List $documentLib -PageSize $pageSize -ScriptBlock { 
                Param($items)
                #$items.Context.ExecuteQuery() 
            
                #Showing data read process along with % completion
                $i = $Global:iCnt
                $progress = [int]($i/$cnt * 100)
                Write-Progress -Activity "Processed up to $i items out of $cnt" -Status "$progress% Complete:" -PercentComplete $progress -id 1
                
                $recordPosition = $Global:iCnt +  $items.Count

                if($recordPosition -gt $startingRecord -and $recordPosition -le $endingRecord)
                {
                    #Create file for each chunk of data to reduce file size
                    if(($Global:iCnt - $startingRecord + 1) % $Global:maxItemsPerCsv -eq 0)
                    {
                        #Setting start value to number of items processed + 1
                        $startVal = $Global:iCnt + 1
                        #Setting end value to number of items processed + chunk size
                        $endVal = $Global:iCnt + $Global:maxItemsPerCsv
                        #If end value is greater than number of records then reset the endval to items count
                        if($endVal -gt $cnt)
                        {
                            $endVal = $cnt
                        }
                        $Global:fileName = GetFileName $startVal $endVal
                    }
                    ReadListItems $items
                }
                elseif($Global:iCnt -gt $endingRecord)
                {
                    WriteLog ("Execution stopped as required data is already processed")
                    exit
                }
                else
                {
                    WriteLog ("Skipped " + $recordPosition + " items out of $cnt ")
                }
                
                $Global:iCnt = $recordPosition             
            }
            WriteLog "Report copied to: $ReportLocation"
        }
    }
    catch
    {
        WriteLog ("Error while processing. Exception: " + $_.Exception.Message)
        WriteLog ("Stack trace: " + $_.Exception.StackTrace)
    }
    finally
    {
        Write-Progress -Activity "Completed!" -Completed $true -id 1
    }
    WriteLog "Report generation completed!"
}

Main