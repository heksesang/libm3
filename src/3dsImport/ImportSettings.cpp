// ImportSettings.cpp : implementation file
//

#include "stdafx.h"
#include "3dsImport.h"
#include "ImportSettings.h"
#include "afxdialogex.h"


// ImportSettings dialog

IMPLEMENT_DYNAMIC(ImportSettings, CDialogEx)

ImportSettings::ImportSettings(CWnd* pParent /*=NULL*/)
	: CDialogEx(ImportSettings::IDD, pParent)
{

}

ImportSettings::~ImportSettings()
{
}

void ImportSettings::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(ImportSettings, CDialogEx)
    ON_LBN_SELCHANGE(IDC_LIST_REGN, &ImportSettings::OnLbnSelchangeListRegn)
END_MESSAGE_MAP()


// ImportSettings message handlers


void ImportSettings::OnLbnSelchangeListRegn()
{
    // TODO: Add your control notification handler code here
}
