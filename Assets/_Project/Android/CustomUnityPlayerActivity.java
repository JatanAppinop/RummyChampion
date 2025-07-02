package com.appinon.rummychampion;

import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayerActivity;

import android.net.Uri;
import android.os.Build;
import androidx.core.content.FileProvider;
import android.app.AlertDialog;
import android.provider.Settings;

import java.io.File;

import com.unity3d.player.UnityPlayer;
import com.appinop.securewebview.R;


public class CustomUnityPlayerActivity extends UnityPlayerActivity {

    private static CustomUnityPlayerActivity instance;
    private static final int REQUEST_INSTALL_UNKNOWN_APP_SOURCES = 1234;
    private String filePath;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        instance = this;
    }

    public static CustomUnityPlayerActivity getInstance() {
        return instance;
    }

    public void OpenNewVersion(String location) {
        this.filePath = location;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            boolean canInstall = getPackageManager().canRequestPackageInstalls();
            if (!canInstall) {
                // Show dialog explaining why permission is needed and how to enable it
                new AlertDialog.Builder(this, R.style.AlertDialogDark)
                    .setTitle("Enable Installation Permission")
                    .setMessage("Please enable 'Allow from this source' permission for this app to install the update.")
                    .setPositiveButton("Settings", (dialog, which) -> {
                        Intent intent = new Intent(Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES,
                                Uri.parse("package:" + getPackageName()));
                        startActivityForResult(intent, REQUEST_INSTALL_UNKNOWN_APP_SOURCES);
                    })
                    .setNegativeButton("Cancel", (dialog, which) -> dialog.dismiss())
                    .show();
                return;
            }
        }

        installApk(filePath);
    }

    private void installApk(String location) {
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setDataAndType(getUriFromFile(location), "application/vnd.android.package-archive");
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        startActivity(intent);
    }

    private Uri getUriFromFile(String filePath) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.N) {
            return Uri.fromFile(new File(filePath));
        } else {
            return FileProvider.getUriForFile(this, getPackageName() + ".provider", new File(filePath));
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == REQUEST_INSTALL_UNKNOWN_APP_SOURCES) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                boolean canInstall = getPackageManager().canRequestPackageInstalls();
                if (canInstall) {
                    installApk(filePath); // Retry installing the APK after permission is granted
                } else {
                    // Show message to user indicating that permission was not granted
                    new AlertDialog.Builder(this)
                        .setTitle("Permission Denied")
                        .setMessage("Permission to install unknown apps was not granted. Unable to install the update.")
                        .setPositiveButton("OK", (dialog, which) -> dialog.dismiss())
                        .show();
                }
            }
        } else {
            super.onActivityResult(requestCode, resultCode, data);
        }

        if (requestCode == 521 && resultCode == RESULT_OK) { // Change requestCode as needed
            String selectedDate = data.getStringExtra("selectedDate");
            UnityPlayer.UnitySendMessage("DatePicker_Helper", "OnActivityResult", selectedDate);
        }
    }
}
