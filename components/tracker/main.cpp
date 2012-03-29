
#include <windows.h>
#include <stdlib.h>
#include <stdio.h>
#include <tchar.h>
#include <Rpc.h>

#include <XnOpenNI.h>
#include <XnCodecIDs.h>
#include <XnCppWrapper.h>
#include <XnPropNames.h>
#include <gl/glut.h>
#include <simpleini/SimpleIni.h>
#include "scenedrawer.h"
#include "diagnostics.h"
#include "utilities.h"

#define GL_WIN_SIZE_X 720
#define GL_WIN_SIZE_Y 480
#define XN_CALIBRATION_FILE_NAME "calibration.bin"
#define CFG_OPENNI_XML "openni.xml"
#define CFG_INI "tracker.ini"
#define USER_TRACK_COUNT_MAX 100
#define STATISTICS_TIMER_INTERVAL_SEC 5
#define STATISTICS_FILE_NAME_MAX_COUNT 1024

#define CHECK_RC(nRetVal, what)									\
if (nRetVal != XN_STATUS_OK)									\
{																\
	Trace("%s failed: %s\n", what, xnGetStatusString(nRetVal));	\
	return nRetVal;												\
}

typedef struct _USER_PROFILE
{
    UUID ID;
	DWORD InTick;
	DWORD OutTick;
	_USER_PROFILE* Next;
} USER_PROFILE, *PUSER_PROFILE;

xn::Context g_Context;
xn::ScriptNode g_scriptNode;
xn::DepthGenerator g_DepthGenerator;
xn::UserGenerator g_UserGenerator;
xn::Player g_Player;

XnBool g_bNeedPose = FALSE;
XnChar g_strPose[20] = "";
XnBool g_bDrawBackground = TRUE;
XnBool g_bDrawPixels = TRUE;
XnBool g_bDrawSkeleton = TRUE;
XnBool g_bPrintID = TRUE;
XnBool g_bPrintState = TRUE;

XnBool g_bPause = false;
XnBool g_bRecord = false;

XnBool g_bQuit = false;

const char * g_strStatisticFile = NULL;
WORD g_cUserInKey;
WORD g_cUserOutKey;
UUID g_CurUsers[USER_TRACK_COUNT_MAX];
PUSER_PROFILE g_pUser = NULL;
UUID g_nilUUID;
UINT_PTR g_nIDTimer;

WCHAR g_strStatFile[STATISTICS_FILE_NAME_MAX_COUNT];
FILE *g_StatStream = NULL;

// Callback: New user was detected
void XN_CALLBACK_TYPE User_NewUser(xn::UserGenerator& generator, XnUserID nId, void* pCookie)
{
	XnUInt32 epochTime = 0;
	xnOSGetEpochTime(&epochTime);
	Trace("%d New User %d\n", epochTime, nId);
	
	SendKey(g_cUserInKey);

	int index = nId;
	if (index >= USER_TRACK_COUNT_MAX)
	{
		Trace("Cannot track user %d\n", nId);
	}
	else
	{
		UUID uuid;
		ZeroMemory(&uuid, sizeof(UUID));
		UuidCreate(&uuid);
		g_CurUsers[index] = uuid;

		PUSER_PROFILE puser = (PUSER_PROFILE)malloc(sizeof(USER_PROFILE));
		ZeroMemory(puser, sizeof(USER_PROFILE));
		puser->ID = g_CurUsers[index];
		puser->InTick = GetTickCount();
		puser->OutTick = 0;
		puser->Next = NULL;

		if (g_pUser == NULL)
		{
			g_pUser = puser;
		}
		else
		{
			puser->Next = g_pUser;
			g_pUser = puser;
		}
	}

	// New user found
	if (g_bNeedPose)
	{
		g_UserGenerator.GetPoseDetectionCap().StartPoseDetection(g_strPose, nId);
	}
	else
	{
		g_UserGenerator.GetSkeletonCap().RequestCalibration(nId, TRUE);
	}
}

// Callback: An existing user was lost
void XN_CALLBACK_TYPE User_LostUser(xn::UserGenerator& generator, XnUserID nId, void* pCookie)
{
	XnUInt32 epochTime = 0;
	xnOSGetEpochTime(&epochTime);
	Trace("%d Lost user %d\n", epochTime, nId);	

	SendKey(g_cUserOutKey);
	
	int index = nId;
	if (index >= USER_TRACK_COUNT_MAX)
	{
		Trace("Cannot track user %d\n", nId);
	}
	else
	{
		RPC_STATUS status;
		UUID uuid = g_CurUsers[index];
		if (UuidIsNil(&uuid, &status))
		{
			Trace("Cannot find user %d\n", nId);
		}
		else
		{
			PUSER_PROFILE p = g_pUser;
			while (p != NULL)
			{
				if (UuidEqual(&uuid, &(p->ID), &status))
				{
					break;
				}
				else
				{
					p = p->Next;
				}
			}
			if (p == NULL)
			{
				Trace("Cannot find user %d\n", nId);
			}
			else
			{
				p->OutTick = GetTickCount();
			}
			g_CurUsers[index] = g_nilUUID;
		}
	}
}

// Callback: Detected a pose
void XN_CALLBACK_TYPE UserPose_PoseDetected(xn::PoseDetectionCapability& capability, const XnChar* strPose, XnUserID nId, void* pCookie)
{
	XnUInt32 epochTime = 0;
	xnOSGetEpochTime(&epochTime);
	Trace("%d Pose %s detected for user %d\n", epochTime, strPose, nId);
	g_UserGenerator.GetPoseDetectionCap().StopPoseDetection(nId);
	g_UserGenerator.GetSkeletonCap().RequestCalibration(nId, TRUE);
}

// Callback: Started calibration
void XN_CALLBACK_TYPE UserCalibration_CalibrationStart(xn::SkeletonCapability& capability, XnUserID nId, void* pCookie)
{
	XnUInt32 epochTime = 0;
	xnOSGetEpochTime(&epochTime);
	Trace("%d Calibration started for user %d\n", epochTime, nId);
}

// Callback: Finished calibration
void XN_CALLBACK_TYPE UserCalibration_CalibrationComplete(xn::SkeletonCapability& capability, XnUserID nId, XnCalibrationStatus eStatus, void* pCookie)
{
	XnUInt32 epochTime = 0;
	xnOSGetEpochTime(&epochTime);
	if (eStatus == XN_CALIBRATION_STATUS_OK)
	{
		// Calibration succeeded
		Trace("%d Calibration complete, start tracking user %d\n", epochTime, nId);		
		g_UserGenerator.GetSkeletonCap().StartTracking(nId);
	}
	else
	{
		// Calibration failed
		Trace("%d Calibration failed for user %d\n", epochTime, nId);
        if(eStatus==XN_CALIBRATION_STATUS_MANUAL_ABORT)
        {
            Trace("Manual abort occured, stop attempting to calibrate!");
            return;
        }
		if (g_bNeedPose)
		{
			g_UserGenerator.GetPoseDetectionCap().StartPoseDetection(g_strPose, nId);
		}
		else
		{
			g_UserGenerator.GetSkeletonCap().RequestCalibration(nId, TRUE);
		}
	}
}

// Save calibration to file
void SaveCalibration()
{
	XnUserID aUserIDs[20] = {0};
	XnUInt16 nUsers = 20;
	g_UserGenerator.GetUsers(aUserIDs, nUsers);
	for (int i = 0; i < nUsers; ++i)
	{
		// Find a user who is already calibrated
		if (g_UserGenerator.GetSkeletonCap().IsCalibrated(aUserIDs[i]))
		{
			// Save user's calibration to file
			g_UserGenerator.GetSkeletonCap().SaveCalibrationDataToFile(aUserIDs[i], XN_CALIBRATION_FILE_NAME);
			break;
		}
	}
}

// Load calibration from file
void LoadCalibration()
{
	XnUserID aUserIDs[20] = {0};
	XnUInt16 nUsers = 20;
	g_UserGenerator.GetUsers(aUserIDs, nUsers);
	for (int i = 0; i < nUsers; ++i)
	{
		// Find a user who isn't calibrated or currently in pose
		if (g_UserGenerator.GetSkeletonCap().IsCalibrated(aUserIDs[i])) continue;
		if (g_UserGenerator.GetSkeletonCap().IsCalibrating(aUserIDs[i])) continue;

		// Load user's calibration from file
		XnStatus rc = g_UserGenerator.GetSkeletonCap().LoadCalibrationDataFromFile(aUserIDs[i], XN_CALIBRATION_FILE_NAME);
		if (rc == XN_STATUS_OK)
		{
			// Make sure state is coherent
			g_UserGenerator.GetPoseDetectionCap().StopPoseDetection(aUserIDs[i]);
			g_UserGenerator.GetSkeletonCap().StartTracking(aUserIDs[i]);
		}
		break;
	}
}

void CleanupExit()
{
	g_scriptNode.Release();
	g_DepthGenerator.Release();
	g_UserGenerator.Release();
	g_Player.Release();
	g_Context.Release();

	if (g_nIDTimer != 0)
	{
		KillTimer(NULL, g_nIDTimer);
	}

	if (g_StatStream == NULL)
	{
		fclose(g_StatStream);
		g_StatStream = NULL;
	}

	exit (1);
}

// this function is called each frame
void glutDisplay (void)
{

	glClear (GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	// Setup the OpenGL viewpoint
	glMatrixMode(GL_PROJECTION);
	glPushMatrix();
	glLoadIdentity();

	xn::SceneMetaData sceneMD;
	xn::DepthMetaData depthMD;
	g_DepthGenerator.GetMetaData(depthMD);
	glOrtho(0, depthMD.XRes(), depthMD.YRes(), 0, -1.0, 1.0);


	glDisable(GL_TEXTURE_2D);

	if (!g_bPause)
	{
		// Read next available data
		g_Context.WaitOneUpdateAll(g_UserGenerator);
	}

		// Process the data
		g_DepthGenerator.GetMetaData(depthMD);
		g_UserGenerator.GetUserPixels(0, sceneMD);
		DrawDepthMap(depthMD, sceneMD);

	glutSwapBuffers();
}

void glutIdle (void)
{
	if (g_bQuit) {
		CleanupExit();
	}

	// Display the frame
	glutPostRedisplay();
}

void glutKeyboard (unsigned char key, int x, int y)
{
	switch (key)
	{
	case 27:
		CleanupExit();
	case 'b':
		// Draw background?
		g_bDrawBackground = !g_bDrawBackground;
		break;
	case 'x':
		// Draw pixels at all?
		g_bDrawPixels = !g_bDrawPixels;
		break;
	case 's':
		// Draw Skeleton?
		g_bDrawSkeleton = !g_bDrawSkeleton;
		break;
	case 'i':
		// Print label?
		g_bPrintID = !g_bPrintID;
		break;
	case 'l':
		// Print ID & state as label, or only ID?
		g_bPrintState = !g_bPrintState;
		break;
	case'p':
		g_bPause = !g_bPause;
		break;
	case 'S':
		SaveCalibration();
		break;
	case 'L':
		LoadCalibration();
		break;
	}
}

void glInit (int * pargc, char ** argv)
{
	glutInit(pargc, argv);
	glutInitDisplayMode(GLUT_RGB | GLUT_DOUBLE | GLUT_DEPTH);
	glutInitWindowSize(GL_WIN_SIZE_X, GL_WIN_SIZE_Y);
	glutCreateWindow ("User Tracker Viewer");
	//glutFullScreen();
	glutSetCursor(GLUT_CURSOR_NONE);

	glutKeyboardFunc(glutKeyboard);
	glutDisplayFunc(glutDisplay);
	glutIdleFunc(glutIdle);

	glDisable(GL_DEPTH_TEST);
	glEnable(GL_TEXTURE_2D);

	glEnableClientState(GL_VERTEX_ARRAY);
	glDisableClientState(GL_COLOR_ARRAY);
}

void CALLBACK StatisticsTimerProc(HWND hwnd, UINT message, UINT idTimer, DWORD dwTime)
{
	Trace("Timer called, %d \n", dwTime);
	
	int userDetectedCount = 0;
	int userInSceneCount = 0;

	int averageTicks = 0;
	int totalTicks = 0;
	PUSER_PROFILE pUser = g_pUser;
	while (pUser != NULL)
	{
		if (pUser->OutTick == 0)
		{
			userInSceneCount ++;
		}
		else
		{
			userDetectedCount ++;
			totalTicks = totalTicks + pUser->OutTick - pUser->InTick;
		}
		pUser = pUser->Next;
	}
	//averageTicks = totalTicks / userDetectedCount;
	fseek(g_StatStream, 0L, SEEK_SET);
	fwprintf_s(g_StatStream, TEXT("userDetectedCount:%d\n"), userDetectedCount);
	fwprintf_s(g_StatStream, TEXT("userInSceneCount:%d\n"), userInSceneCount);
	fwprintf_s(g_StatStream, TEXT("totalTicks:%d\n"), totalTicks);
	//fwprintf_s(g_StatStream, TEXT("averageTicks:%d\n"), averageTicks);
	fflush(g_StatStream);
}


int main(int argc, char **argv)
{

	XnStatus nRetVal = XN_STATUS_OK;

	for(int i = 0; i < USER_TRACK_COUNT_MAX; i ++)
	{
		UuidCreateNil(&g_CurUsers[i]);
	}
	
	UuidCreateNil(&g_nilUUID); 
		
	CSimpleIniA ini;
	ini.SetUnicode();
	if (SI_Error::SI_OK != ini.LoadFile(CFG_INI))
	{
		Trace("Fail to read tracker configuration \n");
		return 1;
	}
	
	const char* strStatFile = ini.GetValue("statistic", "output-file", "C:\statistics.html");
	if (strStatFile == NULL)
	{
		Trace("Fail to read statistic output file configuration \n");
		return 1;
	}

	AnsiToUnicode16L(strStatFile, strlen(strStatFile), g_strStatFile, STATISTICS_FILE_NAME_MAX_COUNT);
	g_StatStream = _wfsopen(g_strStatFile, TEXT("wt"), _SH_DENYWR);
	if (g_StatStream == NULL)
	{
		Trace("Fail to read statistic output file configuration \n");
		return 1;
	}

	const char* strUserInKey = ini.GetValue("keyboard-mapping", "user-in", "1");
	if (strUserInKey == NULL)
	{
		Trace("Fail to read user in key configuratin \n");
		return 1;
	}
	const char* strUserOutKey = ini.GetValue("keyboard-mapping", "user-out", "2");
	if (strUserOutKey == NULL)
	{
		Trace("Fail to read user in key configuratin \n");
		return 1;
	}
	WCHAR inKey;
	AnsiToUnicode16L(strUserInKey, strlen(strUserInKey), &inKey, 2);
	WCHAR outKey;
	AnsiToUnicode16L(strUserOutKey, strlen(strUserOutKey), &outKey, 2);

	g_cUserInKey = inKey;
	g_cUserOutKey = outKey;



	g_nIDTimer = SetTimer(NULL, 0, STATISTICS_TIMER_INTERVAL_SEC * 1000, (TIMERPROC)StatisticsTimerProc);


	if (argc > 1)
	{
		nRetVal = g_Context.Init();
		CHECK_RC(nRetVal, "Init");
		nRetVal = g_Context.OpenFileRecording(argv[1], g_Player);
		if (nRetVal != XN_STATUS_OK)
		{
			Trace("Can't open recording %s: %s\n", argv[1], xnGetStatusString(nRetVal));
			return 1;
		}
	}
	else
	{
		xn::EnumerationErrors errors;
		nRetVal = g_Context.InitFromXmlFile(CFG_OPENNI_XML, g_scriptNode, &errors);
		if (nRetVal == XN_STATUS_NO_NODE_PRESENT)
		{
			XnChar strError[1024];
			errors.ToString(strError, 1024);
			Trace("%s\n", strError);
			return (nRetVal);
		}
		else if (nRetVal != XN_STATUS_OK)
		{
			Trace("Open failed: %s\n", xnGetStatusString(nRetVal));
			return (nRetVal);
		}
	}
	
	nRetVal = g_Context.FindExistingNode(XN_NODE_TYPE_DEPTH, g_DepthGenerator);
	if (nRetVal != XN_STATUS_OK)
	{
		Trace("No depth generator found. Using a default one...");
		xn::MockDepthGenerator mockDepth;
		nRetVal = mockDepth.Create(g_Context);
		CHECK_RC(nRetVal, "Create mock depth");

		// set some defaults
		XnMapOutputMode defaultMode;
		defaultMode.nXRes = 320;
		defaultMode.nYRes = 240;
		defaultMode.nFPS = 30;
		nRetVal = mockDepth.SetMapOutputMode(defaultMode);
		CHECK_RC(nRetVal, "set default mode");

		// set FOV
		XnFieldOfView fov;
		fov.fHFOV = 1.0225999419141749;
		fov.fVFOV = 0.79661567681716894;
		nRetVal = mockDepth.SetGeneralProperty(XN_PROP_FIELD_OF_VIEW, sizeof(fov), &fov);
		CHECK_RC(nRetVal, "set FOV");

		XnUInt32 nDataSize = defaultMode.nXRes * defaultMode.nYRes * sizeof(XnDepthPixel);
		XnDepthPixel* pData = (XnDepthPixel*)xnOSCallocAligned(nDataSize, 1, XN_DEFAULT_MEM_ALIGN);

		nRetVal = mockDepth.SetData(1, 0, nDataSize, pData);
		CHECK_RC(nRetVal, "set empty depth map");

		g_DepthGenerator = mockDepth;
	}

	nRetVal = g_Context.FindExistingNode(XN_NODE_TYPE_USER, g_UserGenerator);
	if (nRetVal != XN_STATUS_OK)
	{
		nRetVal = g_UserGenerator.Create(g_Context);
		CHECK_RC(nRetVal, "Find user generator");
	}

	XnCallbackHandle hUserCallbacks, hCalibrationStart, hCalibrationComplete, hPoseDetected, hCalibrationInProgress, hPoseInProgress;
	if (!g_UserGenerator.IsCapabilitySupported(XN_CAPABILITY_SKELETON))
	{
		Trace("Supplied user generator doesn't support skeleton\n");
		return 1;
	}
	nRetVal = g_UserGenerator.RegisterUserCallbacks(User_NewUser, User_LostUser, NULL, hUserCallbacks);
	CHECK_RC(nRetVal, "Register to user callbacks");
	nRetVal = g_UserGenerator.GetSkeletonCap().RegisterToCalibrationStart(UserCalibration_CalibrationStart, NULL, hCalibrationStart);
	CHECK_RC(nRetVal, "Register to calibration start");
	nRetVal = g_UserGenerator.GetSkeletonCap().RegisterToCalibrationComplete(UserCalibration_CalibrationComplete, NULL, hCalibrationComplete);
	CHECK_RC(nRetVal, "Register to calibration complete");

	if (g_UserGenerator.GetSkeletonCap().NeedPoseForCalibration())
	{
		g_bNeedPose = TRUE;
		if (!g_UserGenerator.IsCapabilitySupported(XN_CAPABILITY_POSE_DETECTION))
		{
			Trace("Pose required, but not supported\n");
			return 1;
		}
		nRetVal = g_UserGenerator.GetPoseDetectionCap().RegisterToPoseDetected(UserPose_PoseDetected, NULL, hPoseDetected);
		CHECK_RC(nRetVal, "Register to Pose Detected");
		g_UserGenerator.GetSkeletonCap().GetCalibrationPose(g_strPose);
	}

	g_UserGenerator.GetSkeletonCap().SetSkeletonProfile(XN_SKEL_PROFILE_ALL);

	nRetVal = g_UserGenerator.GetSkeletonCap().RegisterToCalibrationInProgress(MyCalibrationInProgress, NULL, hCalibrationInProgress);
	CHECK_RC(nRetVal, "Register to calibration in progress");

	nRetVal = g_UserGenerator.GetPoseDetectionCap().RegisterToPoseInProgress(MyPoseInProgress, NULL, hPoseInProgress);
	CHECK_RC(nRetVal, "Register to pose in progress");

	nRetVal = g_Context.StartGeneratingAll();
	CHECK_RC(nRetVal, "StartGenerating");

	
	if (g_nIDTimer == 0)
	{
		Trace("Cannot start statistics timer \n");
		CleanupExit();
	}
	else
	{
		glInit(&argc, argv);
		glutMainLoop();
	}
}