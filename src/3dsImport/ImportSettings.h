#pragma once


// ImportSettings dialog

class ImportSettings : public CDialogEx
{
	DECLARE_DYNAMIC(ImportSettings)

public:
	ImportSettings(CWnd* pParent = NULL);   // standard constructor
	virtual ~ImportSettings();

// Dialog Data
	enum { IDD = IDD_PANEL };


protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
    afx_msg void OnBnClickedButton1();
    afx_msg void OnBnClickedButton2();
    afx_msg void OnBnClickedCheck1();
    afx_msg void OnBnClickedCheck2();
    afx_msg void OnLbnSelchangeList1();
    afx_msg void OnLbnSelchangeListRegn();
};
