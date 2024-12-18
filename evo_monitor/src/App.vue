<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>

<template>
  <q-layout view="hHh lpr fFf">

  <q-header elevated class="bg-primary text-white" height-hint="98">
    <q-toolbar>
      <q-toolbar-title>
        Evo Monitor
      </q-toolbar-title>
    </q-toolbar>

    <q-tabs align="left">
      <q-route-tab to="/" label="Main" />
    </q-tabs>
  </q-header>

  <q-page-container>
    <router-view />
  </q-page-container>

  </q-layout>
</template>
