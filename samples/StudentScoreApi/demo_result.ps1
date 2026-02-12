function Invoke-Api {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [string]$Body = $null
    )

    Write-Host "`n[$Method] $Uri" -ForegroundColor Cyan
    if ($Body) {
        Write-Host "Body: $Body" -ForegroundColor DarkGray
    }

    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            ContentType = "application/json"
            ErrorAction = "Stop"
        }
        if ($Body) { $params.Body = $Body }
        
        $response = Invoke-RestMethod @params -Verbose 4>&1
        Write-Host "Status: Success (2xx)" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 5 | Write-Host
        return $response
    }
    catch {
        $ex = $_.Exception
        if ($ex.Response) {
            $reader = New-Object System.IO.StreamReader($ex.Response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
            Write-Host "Status: $($ex.Response.StatusCode) ($([int]$ex.Response.StatusCode))" -ForegroundColor Red
            Write-Host "Response (Error from Result):" -ForegroundColor Red
            # Try to pretty print JSON if possible
            try {
                $errorBody | ConvertFrom-Json | ConvertTo-Json -Depth 5 | Write-Host
            } catch {
                Write-Host $errorBody
            }
        }
        else {
            Write-Error $ex.Message
        }
    }
}

$baseUrl = "http://localhost:5202"

# 1. Create a Student (Success -> Result.Success)
Write-Host "--- DEMO: Result.Success (Create Student) ---" -ForegroundColor Yellow
$student = Invoke-Api -Uri "$baseUrl/Student" -Method "POST" -Body '"Bob"'

if ($student) {
    $id = $student.id

    # 2. Add Valid Score (Success -> Result.Success)
    Write-Host "`n--- DEMO: Result.Success (Add Score) ---" -ForegroundColor Yellow
    Invoke-Api -Uri "$baseUrl/Student/$id/scores" -Method "POST" -Body '85'

    # 3. Add Invalid Score (Failure -> Result.Failure with Validation Error)
    Write-Host "`n--- DEMO: Result.Failure (Add Invalid Score) ---" -ForegroundColor Yellow
    Invoke-Api -Uri "$baseUrl/Student/$id/scores" -Method "POST" -Body '105'
}

# 4. Get Non-Existent Student (Failure -> Result.Failure with NotFound Error)
Write-Host "`n--- DEMO: Result.Failure (Get Non-Existent Student) ---" -ForegroundColor Yellow
$badId = [Guid]::NewGuid()
Invoke-Api -Uri "$baseUrl/Student/$badId"
