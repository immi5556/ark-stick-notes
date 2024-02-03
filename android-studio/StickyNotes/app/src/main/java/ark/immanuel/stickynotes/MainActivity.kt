package ark.immanuel.stickynotes

import android.Manifest
import android.annotation.SuppressLint
import android.app.Activity
import android.app.DownloadManager
import android.app.ProgressDialog
import android.content.Context
import androidx.appcompat.app.AppCompatActivity
import android.widget.Toast
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import android.net.Uri
import android.os.*
import android.util.Log
import android.view.View
import android.webkit.*
import android.webkit.WebView
import androidx.activity.result.contract.ActivityResultContracts
import androidx.annotation.RequiresApi
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout








class MainActivity : AppCompatActivity() {
    private val webView: WebView? = null
    lateinit var swipeRefreshLayout: SwipeRefreshLayout
private val activity: MainActivity = this
    private val requestPermissionLauncher =
        registerForActivityResult(
            ActivityResultContracts.RequestPermission()
        ) { isGranted: Boolean ->
            if (isGranted) {
                Log.i("Permission: ", "Granted")
                Toast.makeText(this, "Storage Permission Granted....", Toast.LENGTH_SHORT).show()
            } else {
                Log.i("Permission: ", "Denied")
                Toast.makeText(this, "Storage Permission Denied....", Toast.LENGTH_SHORT).show()
            }
        }
    fun requestPermission(view: View, perm: Manifest.permission) {
        when {
            ContextCompat.checkSelfPermission(
                this,
                perm.toString()
            ) == PackageManager.PERMISSION_GRANTED -> {
                Toast.makeText(this, "Storage Permission Granted....", Toast.LENGTH_SHORT).show()
            }

            ActivityCompat.shouldShowRequestPermissionRationale(
                this,
                perm.toString()
            ) -> {
                Toast.makeText(this, "Storage Permission Required....", Toast.LENGTH_SHORT).show()
                    requestPermissionLauncher.launch(
                        perm.toString()
                    )
            }
            else -> {
                requestPermissionLauncher.launch(
                    perm.toString()
                )
            }
        }
    }

    @SuppressLint("SetJavaScriptEnabled")
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        val progressDialog: ProgressDialog = ProgressDialog(this)
        val webView = findViewById<WebView>(R.id.webView)
        swipeRefreshLayout = findViewById(R.id.swiperefresh);

        swipeRefreshLayout.setOnRefreshListener {
            swipeRefreshLayout.setRefreshing(false)
            webView.reload()
        }

        swipeRefreshLayout.setColorSchemeColors(
            getResources().getColor(android.R.color.holo_blue_bright),
            getResources().getColor(android.R.color.holo_orange_dark),
            getResources().getColor(android.R.color.holo_green_dark),
            getResources().getColor(android.R.color.holo_red_dark)
        );

        webView.setDownloadListener(DownloadListener {
                url,
                userAgent,
                contentDescription,
                mimetype,
                contentLength ->

            Toast.makeText(this, "Download Listen 1...." + mimetype, Toast.LENGTH_SHORT).show()
            // Initialize download request
            val request = DownloadManager.Request(Uri.parse(url))

            // Get the cookie
            val cookies = CookieManager.getInstance().getCookie(url)

            // Add the download request header
            request.addRequestHeader("Cookie",cookies)
            request.addRequestHeader("User-Agent",userAgent)

            // Set download request description
            request.setDescription("Downloading requested file....")

            // Set download request mime tytpe
            request.setMimeType(mimetype)

            // Allow scanning
            request.allowScanningByMediaScanner()

            // Download request notification setting
            request.setNotificationVisibility(
                DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED)

            // Guess the file name
            var fileName = URLUtil.guessFileName(url, contentDescription, mimetype)
            fileName = fileName.replace(".bin", ".csv", true)
            Toast.makeText(this, "Download Listen 2...." + url, Toast.LENGTH_SHORT).show()
            // Set a destination storage for downloaded file
            request.setDestinationInExternalPublicDir(Environment.DIRECTORY_DOWNLOADS, fileName)

            // Set request title
            //request.setTitle(URLUtil.guessFileName(url, contentDescription, mimetype));
            request.setTitle(fileName)


            // DownloadManager request more settings
            request.setAllowedOverMetered(true)
            request.setAllowedOverRoaming(false)
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                request.setRequiresCharging(false)
                request.setRequiresDeviceIdle(false)
            }
            request.setVisibleInDownloadsUi(true)


            // Get the system download service
            val dManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager

            // Finally, request the download to system download service
            dManager.enqueue(request)
        })

        webView.webViewClient = object : WebViewClient() {
            override fun shouldOverrideUrlLoading(view: WebView?, urlNewString: String?): Boolean {
//                if (!loadingFinished) {
//                    redirect = true
//                }
//                loadingFinished = false
                webView.loadUrl(urlNewString!!)
                return true
            }
            override fun onPageStarted(view: WebView?, url: String?, favicon: Bitmap?) {
                println("Load Started: " + url)
                progressDialog.setTitle("Please Wait")
                progressDialog.setMessage("Loading ...")
                progressDialog.setCancelable(false) // blocks UI interaction
                progressDialog.show()
            }
            override fun onPageFinished(view: WebView, url: String) {
                println("Load Finished: " + url)
                progressDialog.hide()
            }

            override fun onReceivedError(
                view: WebView?,
                request: WebResourceRequest?,
                error: WebResourceError?
            ) {
                println("Load Errored: ")
                progressDialog.hide()
                super.onReceivedError(view, request, error)
            }
        }
        webView.webChromeClient = object : WebChromeClient() {
            @RequiresApi(Build.VERSION_CODES.M)
            override fun onPermissionRequest(request: PermissionRequest) {
                Log.i("Permissions", "onPermissionRequest")

                // grants permission for app. video not showing
                if (ContextCompat.checkSelfPermission(activity, Manifest.permission.WRITE_EXTERNAL_STORAGE)
                    != PackageManager.PERMISSION_GRANTED
                ) {
                    Log.i("Permissions", "Request Permission")
                    requestPermissions(arrayOf(Manifest.permission.WRITE_EXTERNAL_STORAGE, Manifest.permission.MODIFY_AUDIO_SETTINGS, Manifest.permission.CAPTURE_AUDIO_OUTPUT, Manifest.permission.RECORD_AUDIO), 1010)
                } else {
                    Log.i("Permissions", "Permission already granted")
                }
            }
        }
        // this will enable the javascript settings, it can also allow xss vulnerabilities
        webView.settings.javaScriptEnabled = true
        webView.addJavascriptInterface(MyJavascriptInterface(this), "MyJavascriptInterface")
        if (checkForInternet(this)){
            webView.loadUrl("https://sticky-notes.immanuel.co/record")
        } else {
            webView.loadUrl("file:///android_asset/no_internet.html")
            fun hh() {
                Handler(Looper.getMainLooper()).postDelayed({
                    if (checkForInternet(this)){
                        webView.loadUrl("https://sticky-notes.immanuel.co/record")
                    } else {
                        hh();
                        Toast.makeText(this, "Checking Internet Connection....", Toast.LENGTH_SHORT).show()
                    }
                }, 15000)
            }
            hh()
        }
        // if you want to enable zoom feature
        //webView.settings.setSupportZoom(true)
        //myWebView.loadUrl("https://www.immanuel.co")
        //myWebView.loadUrl("https://sticky-notes.immanuel.co/record")

    }

    override fun onRequestPermissionsResult(requestCode: Int, permissions: Array<String>, grantResults: IntArray) {
        when (requestCode) {
            1010 -> {
                Log.d("MainActivity", "onRequestPermissionsResult: Camera Request")
                if ((grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED)) {
                    Log.d("MainActivity", "Camera Request: Permission granted")
                    // permission was granted, yay!
                } else {
                    // permission denied, boo!
                    Log.d("MainActivity", "Camera Request: Permission denied")
                }
                return
            }
        }
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
    }

    class MyJavascriptInterface(private val context: Context) {
        @JavascriptInterface
        fun showToast(message: String) {
            Toast.makeText(context, message, Toast.LENGTH_SHORT).show()
        }
    }
    // if you press Back button this code will work
    override fun onBackPressed() {
        // if your webview can go back it will go back
        when {
            webView?.canGoBack() == true -> webView.goBack()
            else -> super.onBackPressed()
        }
    }

    @SuppressLint("ServiceCast")
    private fun checkForInternet(context: Context): Boolean {

        // register activity with the connectivity manager service
        val connectivityManager = context.getSystemService(Context.CONNECTIVITY_SERVICE) as ConnectivityManager

        // if the android version is equal to M
        // or greater we need to use the
        // NetworkCapabilities to check what type of
        // network has the internet connection
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {

            // Returns a Network object corresponding to
            // the currently active default data network.
            val network = connectivityManager.activeNetwork ?: return false

            // Representation of the capabilities of an active network.
            val activeNetwork = connectivityManager.getNetworkCapabilities(network) ?: return false

            return when {
                // Indicates this network uses a Wi-Fi transport,
                // or WiFi has network connectivity
                activeNetwork.hasTransport(NetworkCapabilities.TRANSPORT_WIFI) -> true

                // Indicates this network uses a Cellular transport. or
                // Cellular has network connectivity
                activeNetwork.hasTransport(NetworkCapabilities.TRANSPORT_CELLULAR) -> true

                // else return false
                else -> false
            }
        } else {
            // if the android version is below M
            @Suppress("DEPRECATION") val networkInfo =
                connectivityManager.activeNetworkInfo ?: return false
            @Suppress("DEPRECATION")
            return networkInfo.isConnected
        }
    }
}