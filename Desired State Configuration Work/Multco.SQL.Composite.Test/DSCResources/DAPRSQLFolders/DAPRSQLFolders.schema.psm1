Configuration DAPRSQLFolders {
    param
	(
		[Parameter(Mandatory)]
		[string]$DAPRSQLInstances
	)
	Import-DscResource -ModuleName PSDesiredStateConfiguration
    Credential - $PsDscRunAsCredential
	$DAPRSQLJson = $DAPRSQLInstances|ConvertFrom-Json

	Foreach ($DAPRSQLInstance in $DAPRSQLJson.Instances)
    {
		$DAPRInstanceName = $DAPRSQLInstance.SQLInstance
		File Test 
		{
		    DestinationPath = "\\multforest\db_data\Backup\SBX\SQL703SBX\SQLSVR"
            #Recurse = $true
		    Type = "Directory"
		    Ensure = "Present"
		}
     }
	                          #}
}
# SIG # Begin signature block
# MIIQqAYJKoZIhvcNAQcCoIIQmTCCEJUCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQULEV9PlkLIdMYijISMfrEpWAv
# 9qGggg3yMIIGzDCCBLSgAwIBAgITGwAASoDrYDrKV7D/2QADAABKgDANBgkqhkiG
# 9w0BAQsFADBoMRIwEAYKCZImiZPyLGQBGRYCdXMxEjAQBgoJkiaJk/IsZAEZFgJv
# cjEZMBcGCgmSJomT8ixkARkWCW11bHRub21haDESMBAGCgmSJomT8ixkARkWAmNv
# MQ8wDQYDVQQDEwZNVUxUQ0EwHhcNMTkxMTIzMDAzMjU4WhcNMjAxMTIyMDAzMjU4
# WjAzMTEwLwYDVQQDEyhNdWx0bm9tYWggQ291bnR5IFBvd2VyU2hlbGwgQ29kZSBT
# aWduaW5nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqdArWgXa7+lC
# pRkhJSTZgrcDG1RqG9SCFeBERVFbgvps5kDzaIx+xMkBoFB8l9hmmhRGYbfkzMtj
# q+6mTXHdkaFCrVgXAIxwE2Yoe6PbWgvo7SAXk2eFYKyS0eyndlPxURMHmJLRJoVy
# 5gKuoipLDN9C3DjJBYWiS0RRU87zp41x02g85Ypz/9ay646POA3dk3c1xQwiqCHE
# h7ag26RzufbsCirYBaU6CV1H49Tr22zAMERF1xLK7kHF01WZlD5colZFJF3ZZgGM
# 0z9vXDiOsQdDreniKZq4XpIYnsEbELd6Ola2Cl65adznzqF+l1VE9/4ShrhcrUAr
# HndwXO86WQIDAQABo4ICojCCAp4wPgYJKwYBBAGCNxUHBDEwLwYnKwYBBAGCNxUI
# h9DncIXJiSOCvZ0cho3WP4fNm22BD4af7B2GxolgAgFkAgECMBMGA1UdJQQMMAoG
# CCsGAQUFBwMDMAsGA1UdDwQEAwIHgDAMBgNVHRMBAf8EAjAAMBsGCSsGAQQBgjcV
# CgQOMAwwCgYIKwYBBQUHAwMwHQYDVR0OBBYEFIZTT4x6eJHMD9sAitvMrgf/OpBU
# MB8GA1UdIwQYMBaAFIFlKiDZdYFdTXda4otUT1/zMO+8MIIBBQYDVR0fBIH9MIH6
# MIH3oIH0oIHxhoG8bGRhcDovLy9DTj1NVUxUQ0EoMyksQ049bXVsdGNhLENOPUNE
# UCxDTj1QdWJsaWMlMjBLZXklMjBTZXJ2aWNlcyxDTj1TZXJ2aWNlcyxDTj1Db25m
# aWd1cmF0aW9uLERDPWNvLERDPW11bHRub21haCxEQz1vcixEQz11cz9jZXJ0aWZp
# Y2F0ZVJldm9jYXRpb25MaXN0P2Jhc2U/b2JqZWN0Q2xhc3M9Y1JMRGlzdHJpYnV0
# aW9uUG9pbnSGMGh0dHA6Ly9jcmwuY28ubXVsdG5vbWFoLm9yLnVzL2NybGQvTVVM
# VENBKDMpLmNybDCBxQYIKwYBBQUHAQEEgbgwgbUwgbIGCCsGAQUFBzAChoGlbGRh
# cDovLy9DTj1NVUxUQ0EsQ049QUlBLENOPVB1YmxpYyUyMEtleSUyMFNlcnZpY2Vz
# LENOPVNlcnZpY2VzLENOPUNvbmZpZ3VyYXRpb24sREM9Y28sREM9bXVsdG5vbWFo
# LERDPW9yLERDPXVzP2NBQ2VydGlmaWNhdGU/YmFzZT9vYmplY3RDbGFzcz1jZXJ0
# aWZpY2F0aW9uQXV0aG9yaXR5MA0GCSqGSIb3DQEBCwUAA4ICAQDAXZ5fdCK9pHQg
# FkJd4+pHWfiIr2+LSEqG0n+q2+W4AiNizW5BiYLbEm276c7J6cBRC5qRhGODXX6g
# HZDko0TjQxQ7LXmfajRexxSz4Zveoa2XzS5yQ4D90zeSL+q/BdJhbtVoYkMAocwY
# llxaVBELXcFsbu6xac8VtgfXzSgMjbxBC3BC/VXJB+pjOh+Tsiil2XSWZYVuKS6H
# +xhNkbdS00dfqIKXJjH9bovSFRQP93v486HPCtRPMyx94HZ9Xbz1LAu2wcdcmMtS
# nBK/I+GoR7GfXlz+BNbRVhq07SeEC8ZiVr4zZe0CYxxpiAm7lKtAh9ft/emlxMp9
# z0vVhMPHsx8r4kGtkQD54Tc3G1gUiLxT8pMIYEA8J0mZ/Ry0POHiLRJt7OTRX+rW
# ZJvld0rYq4ARP1zdtCxXbN8sNR8p+w0NkI/sfa02RWDqd2RSzkuHecwy4Dg2Cn2/
# 9u1dTPaMrUhwTC1ISkgqRhhxec/trxnrrF2M1JwRTVA/K39JP02S5tel7bhOlGF0
# w4Ie3Df9zZqgGo3RbrkN0FLLCReMQn+qqyPnxvN/xwcm3Czqfvdv0Gmo940DkGNu
# 3k2AXMRQS/4D1o9ZfE/B2zpctPOnSnmsaH0s+5QfYxip5T/bksg+WouKL0of8VcA
# 3TnT/O8M0Vl0VJE1mwjFx5JYAVguTzCCBx4wggUGoAMCAQICEyAAAAAHb9aTBAVB
# 7f8AAAAAAAcwDQYJKoZIhvcNAQELBQAwFTETMBEGA1UEAxMKTVVMVENBUk9PVDAe
# Fw0xNjA0MTIxNzQ1NTlaFw0zNTEyMjIwMDU4NDNaMGgxEjAQBgoJkiaJk/IsZAEZ
# FgJ1czESMBAGCgmSJomT8ixkARkWAm9yMRkwFwYKCZImiZPyLGQBGRYJbXVsdG5v
# bWFoMRIwEAYKCZImiZPyLGQBGRYCY28xDzANBgNVBAMTBk1VTFRDQTCCAiIwDQYJ
# KoZIhvcNAQEBBQADggIPADCCAgoCggIBANhgh01gUdTAzJmIAo2JlO6LAFX2BSWJ
# vvmOV9OKJHTdTxrECT8RGmgzERHijrVGy9Xzm0EDww6xIHI3tHMxEAM8qWBcxd99
# piAg4wftcOYh91V9y/zUf0J6goawCQS0gB8+NgsxZQ/m8BmTi/PzQiaXuF70gQgO
# xjcySQHiebwcAJ4g89BvYfx6pExAR5p8uOrdfoyBKUsaH+oM81cYfhWc9p1FnWb/
# nsn9pumfAqVtWrCi9SvfW3ekYwouYUlpXljEJ9PBjUOxKBZrNdUBwim90ert0yE4
# FbaXvAMlpVfgMLrh6jP49CM0jh5XMM4avGUivmuGGG3xtUYQIxbIkdygO49kq48H
# +Dv9jX4B4psSnIJtRbrLSXZPmWVCfCHca1Vyc6oe698FlIxzV7vtgwlmNadwyAYe
# yF9HkzzfR7OKioeYrrhTDn+zKnEeSg3NfT1tIofOk0wiUC0e9cBrM0+PfDJ0pUe7
# rbyG1WUqXQfYU3XI7cLmY2zyh0Qy1W2j4WWtniHDV6uxIxUwV1+dGdRiuAidhHEI
# uglXPV6r0qZDKxGsmNYPPWIl2Y8U7cTJ8MDW0YyrQp2Y66A4A4+rDjuNrEC0xdz0
# crpX+H6v1yz4rzahJEnpedG3NKvkxCImXNW/JAxrVy3UrtZvruLmzm7BPbLwbVHJ
# DApomuc6/hpfAgMBAAGjggISMIICDjASBgkrBgEEAYI3FQEEBQIDAwADMCMGCSsG
# AQQBgjcVAgQWBBTlgQXuB980SO3NQjBVBoV8J0O8uzAdBgNVHQ4EFgQUgWUqINl1
# gV1Nd1rii1RPX/Mw77wwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBDAEEwCwYDVR0P
# BAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAUv9X5erbfev3OQNgf
# qsBJv6f6IPQwQgYDVR0fBDswOTA3oDWgM4YxaHR0cDovL2NybC5jby5tdWx0bm9t
# YWgub3IudXMvY3JsZC9NVUxUQ0FST09ULmNybDCCARQGCCsGAQUFBwEBBIIBBjCC
# AQIwgbYGCCsGAQUFBzAChoGpbGRhcDovLy9DTj1NVUxUQ0FST09ULENOPUFJQSxD
# Tj1QdWJsaWMlMjBLZXklMjBTZXJ2aWNlcyxDTj1TZXJ2aWNlcyxDTj1Db25maWd1
# cmF0aW9uLERDPWNvLERDPW11bHRub21haCxEQz1vcixEQz11cz9jQUNlcnRpZmlj
# YXRlP2Jhc2U/b2JqZWN0Q2xhc3M9Y2VydGlmaWNhdGlvbkF1dGhvcml0eTBHBggr
# BgEFBQcwAoY7aHR0cDovL3BraS5jby5tdWx0bm9tYWgub3IudXMvQUlBL211bHRj
# YXJvb3RfTVVMVENBUk9PVC5jcnQwDQYJKoZIhvcNAQELBQADggIBABvulJj3tNtC
# hVHlJuB1zPtMa24rWzHimahLpjdDB7e0d/o0DdIPXiwmkTWfcqKdWKa88j1y/54U
# U/9MPAuJtXMueuZgV65PTvI43/W90jXLNUzq7kTV+itYgN0ice6Eb/vJqWofnRSN
# jGJQG/q3/f33j39Zswd2nI9e7fRuubg1oo5fuM+Blv681E4o8vd4YZkuBi2fPzj3
# 22rppIOTf+o1casYMN5db/bbXZ25n9wQAjyNIDla4WTisWJS9ycLDnhkGpVpxeQE
# o1VfYs+wT2xqwSdbL7b6UmMV/PN4laM9c4Ueivwp1zbdV/GJId6uuj20JBD3oG7z
# j21dU4Qh7osf2EFpx0Kn2yHP+J9+cVUMaFvkY3j2IM0qEDMc9WkmCSixp5AdMkgz
# R3SXpHRLU0Ww6Zk+tpTLPS+QB7zOcI8S7MkRRclUPbsQ9YR4lgIHeHbpDgbBc6Er
# IPsnMZkKpdzuzubiuVIhcijeMWDW+ajunUQj39V2aHdvrjYwVM0iJjS1Cu98tGBM
# TCwOh6bYg3HOHHjJ1sYjg54Nx1rUvTti+oBW+UhbbWugFb50O0B1EKm4O1/NHG0+
# 3v92Q+FgMcjtNw4JC1CBjM1yAQu8FZnL+pdPtQ9eeXLlaSo0MtGsNoyscmVXvTvA
# 03JTH29pk+j1tmk5vwS/q6/1DFv0KxQmMYICIDCCAhwCAQEwfzBoMRIwEAYKCZIm
# iZPyLGQBGRYCdXMxEjAQBgoJkiaJk/IsZAEZFgJvcjEZMBcGCgmSJomT8ixkARkW
# CW11bHRub21haDESMBAGCgmSJomT8ixkARkWAmNvMQ8wDQYDVQQDEwZNVUxUQ0EC
# ExsAAEqA62A6ylew/9kAAwAASoAwCQYFKw4DAhoFAKB4MBgGCisGAQQBgjcCAQwx
# CjAIoAKAAKECgAAwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGC
# NwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZIhvcNAQkEMRYEFOqTf4MQiHwt9M4z
# HK+7KSTLTUK+MA0GCSqGSIb3DQEBAQUABIIBAHaz3uAZ0PTBlhW+zZ0PX65kIQwO
# a1VB3EXWeGQ36PLdvGSrQwXW+lwn6R6qmRNGO2+jNRCAm1suQOuwE9nO6Zrln74y
# vm8IddJ8pCcoqsC/BwM9hdRcqLYToUSpFVFSELh177ONj3/v/rbDp+JlNEKXXsSN
# AkuB+Jwr5QYQDTv+GCJs0NF/qb0qnuckm5lSuCx8G675jeTzrs7vIXHqn6Iru5X1
# LWd2Ii2zwot/IkAfn2g1bf8vh5VdjC8yJf++irxhsLqd2q8N0zIghd2vFH9APMVq
# 9krwwki0lgZrejMFl3iUdH3yPbAzGgEAdepnv3WZRdQ402h7PKrb/1768rQ=
# SIG # End signature block
