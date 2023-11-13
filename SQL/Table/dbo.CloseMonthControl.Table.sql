CREATE TABLE CloseMonthControl 
(
	CloseMonth nvarchar(8) NOT NULL,
	CloseStatus nchar(3),
	CreateEmployeeCode nvarchar(50),
	CreateDate datetime,
	LastUpdateEmployeeCode nvarchar(50),
	LastUpdateDate datetime,
	DelFlag nvarchar(2),
	CONSTRAINT [PK_CloseMonthControl] PRIMARY KEY CLUSTERED 
    (
      CloseMonth
    )
)